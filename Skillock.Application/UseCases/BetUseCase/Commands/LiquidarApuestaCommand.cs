using MediatR;
using Skillock.Application.Common;
using Skillock.Application.Interfaces;
using Skillock.Domain.Common;
using Skillock.Domain.DTOs.Responses;
using Skillock.Domain.Enums;
using Skillock.Domain.Models;

namespace Skillock.Application.UseCases.BetUseCase.Commands;

public record LiquidarApuestaCommand(Guid BetId, MatchResult Resultado)
    : IRequest<ApplicationResult<LiquidacionResponse>>;

public class LiquidarApuestaCommandHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<LiquidarApuestaCommand, ApplicationResult<LiquidacionResponse>>
{
    public async Task<ApplicationResult<LiquidacionResponse>> Handle(
        LiquidarApuestaCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var bet = await unitOfWork.Bets.GetWithPartiesAsync(request.BetId, cancellationToken);
            if (bet is null)
                return ApplicationResult<LiquidacionResponse>.NotFound("Bet", request.BetId);

            if (bet.Status != BetStatus.Active)
                return ApplicationResult<LiquidacionResponse>.Failure(
                    "INVALID_STATUS", "La apuesta no está en estado Active.");

            if (request.Resultado == MatchResult.Pending)
                return ApplicationResult<LiquidacionResponse>.Failure(
                    "INVALID_RESULT", "El resultado no puede ser Pending.");

            bet.MatchResult = request.Resultado;

            var premioTotal = bet.BetParties.Sum(bp => bp.MontoAcumulado);
            var platformFee = Math.Round(premioTotal * DomainConstants.PlatformFeePercent, 2, MidpointRounding.ToZero);
            var premioNeto = Math.Round(premioTotal - platformFee, 2, MidpointRounding.ToZero);

            var partyGanadora = bet.ObtenerPartyGanadora();
            var partyPerdedora = bet.ObtenerPartyPerdedora();

            await unitOfWork.BeginTransactionAsync(cancellationToken: cancellationToken);

            var pagos = new List<PagoIndividualResponse>();

            // Procesar ganadores
            if (partyGanadora is not null)
            {
                foreach (var member in partyGanadora.Members.Where(m => m.AporteConfirmado))
                {
                    var proporcion = member.MontoAportado / partyGanadora.MontoAcumulado;
                    var pago = Math.Round(proporcion * premioNeto, 2, MidpointRounding.ToZero);

                    var wallet = await unitOfWork.Wallets.GetByUserIdWithLockAsync(member.UserId, cancellationToken);
                    if (wallet is null) continue;

                    // Usar métodos de dominio — nunca modificar saldos directamente
                    wallet.ConsumirRetenido(member.MontoAportado);
                    wallet.AcreditarPremio(member.MontoAportado, pago);

                    wallet.Transactions.Add(new WalletTransaction
                    {
                        WalletId = wallet.Id,
                        Type = TransactionType.BetPayout,
                        Amount = pago,
                        BalanceAfter = wallet.SaldoDisponible,
                        BetId = bet.Id,
                        Description = $"Premio por victoria en apuesta {bet.Id}"
                    });

                    unitOfWork.Wallets.Update(wallet);

                    pagos.Add(new PagoIndividualResponse(
                        member.UserId,
                        member.User?.Username ?? "Unknown",
                        member.MontoAportado,
                        pago,
                        Math.Round(proporcion, 4, MidpointRounding.ToZero)));
                }
            }

            // Procesar perdedores — solo consumir retenido, sin payout
            if (partyPerdedora is not null)
            {
                foreach (var member in partyPerdedora.Members.Where(m => m.AporteConfirmado))
                {
                    var wallet = await unitOfWork.Wallets.GetByUserIdWithLockAsync(member.UserId, cancellationToken);
                    if (wallet is null) continue;

                    wallet.ConsumirRetenido(member.MontoAportado);
                    unitOfWork.Wallets.Update(wallet);
                }
            }

            // Acreditar fee a wallet de la plataforma usando método de dominio
            var platformWallet = await unitOfWork.Wallets.GetByUserIdWithLockAsync(
                DomainConstants.PlatformWalletId, cancellationToken);

            if (platformWallet is not null)
            {
                platformWallet.AcreditarPlatformFee(platformFee);

                platformWallet.Transactions.Add(new WalletTransaction
                {
                    WalletId = platformWallet.Id,
                    Type = TransactionType.PlatformFee,
                    Amount = platformFee,
                    BalanceAfter = platformWallet.SaldoDisponible,
                    BetId = bet.Id,
                    Description = $"Comisión 7% apuesta {bet.Id}"
                });

                unitOfWork.Wallets.Update(platformWallet);
            }

            bet.Status = BetStatus.Completed;
            bet.CompletedAt = DateTime.UtcNow;

            await unitOfWork.SaveChangesAsync(cancellationToken);
            await unitOfWork.CommitTransactionAsync(cancellationToken);

            return ApplicationResult<LiquidacionResponse>.Success(new LiquidacionResponse(
                bet.Id,
                request.Resultado,
                premioTotal,
                platformFee,
                premioNeto,
                pagos.AsReadOnly(),
                DateTime.UtcNow));
        }
        catch (DomainException ex)
        {
            await unitOfWork.RollbackTransactionAsync(cancellationToken);
            return ApplicationResult<LiquidacionResponse>.Failure("DOMAIN_ERROR", ex.Message);
        }
        catch (Exception)
        {
            await unitOfWork.RollbackTransactionAsync(cancellationToken);
            return ApplicationResult<LiquidacionResponse>.Failure("ERROR", "Error interno del servidor.");
        }
    }
}
