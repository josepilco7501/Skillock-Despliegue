using Hangfire;
using MediatR;
using Microsoft.Extensions.Logging;
using Skillock.Application.Common;
using Skillock.Application.Interfaces;
using Skillock.Domain.Common;
using Skillock.Domain.DTOs.Requests;
using Skillock.Domain.DTOs.Responses;
using Skillock.Domain.Enums;
using Skillock.Domain.Models;

namespace Skillock.Application.UseCases.BetUseCase.Commands;

public record AportarFondosCommand(AportarFondosRequest Request, Guid UserId)
    : IRequest<ApplicationResult<AporteResponse>>;

public class AportarFondosCommandHandler(
    IUnitOfWork unitOfWork,
    IBackgroundJobClient backgroundJobClient,
    ILogger<AportarFondosCommandHandler> logger) : IRequestHandler<AportarFondosCommand, ApplicationResult<AporteResponse>>
{
    public async Task<ApplicationResult<AporteResponse>> Handle(AportarFondosCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var req = request.Request;

            if (req.Monto <= 0 || req.Monto > DomainConstants.MontoMaximoApuesta)
                return ApplicationResult<AporteResponse>.Failure("INVALID_AMOUNT", $"Monto debe estar entre 0 y {DomainConstants.MontoMaximoApuesta}.");

            var bet = await unitOfWork.Bets.GetWithPartiesAsync(req.BetId, cancellationToken);
            if (bet is null)
                return ApplicationResult<AporteResponse>.NotFound("Bet", req.BetId);

            if (DateTime.UtcNow > bet.ExpiresAt)
                return ApplicationResult<AporteResponse>.Failure("BET_EXPIRED", "La apuesta ha expirado.");

            if (bet.Status != BetStatus.Funding)
                return ApplicationResult<AporteResponse>.Failure("INVALID_STATUS", "La apuesta no está en estado Funding.");

            var party = bet.BetParties.FirstOrDefault(bp => bp.Id == req.BetPartyId);
            if (party is null)
                return ApplicationResult<AporteResponse>.NotFound("BetParty", req.BetPartyId);

            var member = party.Members.FirstOrDefault(m => m.UserId == request.UserId);
            if (member is null)
                return ApplicationResult<AporteResponse>.Failure("NOT_MEMBER", "Usuario no es miembro de este equipo.");

            var agreedAmount = bet.AgreedAmountPerTeam ?? 0m;
            var montoRestante = agreedAmount - party.MontoAcumulado;

            member.BetParty = party;
            member.ValidarAporte(req.Monto, montoRestante, agreedAmount);

            await unitOfWork.BeginTransactionAsync(cancellationToken: cancellationToken);

            var wallet = await unitOfWork.Wallets.GetByUserIdWithLockAsync(request.UserId, cancellationToken);
            if (wallet is null)
            {
                await unitOfWork.RollbackTransactionAsync(cancellationToken);
                return ApplicationResult<AporteResponse>.Failure("NO_WALLET", "Usuario no tiene wallet.");
            }

            if (wallet.SaldoDisponible < req.Monto)
            {
                await unitOfWork.RollbackTransactionAsync(cancellationToken);
                return ApplicationResult<AporteResponse>.Failure("INSUFFICIENT_BALANCE", "Saldo insuficiente.");
            }

            wallet.ReservarFondos(req.Monto);

            var transaction = new WalletTransaction
            {
                WalletId = wallet.Id,
                Type = TransactionType.BetContribution,
                Amount = req.Monto,
                BalanceAfter = wallet.SaldoDisponible,
                BetId = bet.Id,
                Description = $"Aporte a apuesta {bet.Id}"
            };

            wallet.Transactions.Add(transaction);
            unitOfWork.Wallets.Update(wallet);

            member.MontoAportado = req.Monto;
            member.AporteConfirmado = true;
            member.FechaAporte = DateTime.UtcNow;

            party.MontoAcumulado += req.Monto;
            party.EstaCompleto = bet.AgreedAmountPerTeam.HasValue && party.MontoAcumulado == bet.AgreedAmountPerTeam.Value;

            var activada = false;
            if (bet.AmbosEquiposCompletos())
            {
                bet.Status = BetStatus.Active;
                bet.ActivatedAt = DateTime.UtcNow;
                activada = true;
            }

            unitOfWork.Bets.Update(bet);
            await unitOfWork.SaveChangesAsync(cancellationToken);
            await unitOfWork.CommitTransactionAsync(cancellationToken);

            if (activada)
            {
                backgroundJobClient.Schedule<IMatchMonitoringJob>(
                    j => j.Execute(bet.Id),
                    TimeSpan.FromMinutes(2));
                backgroundJobClient.Schedule<IMatchTimeoutJob>(
                    j => j.Execute(bet.Id),
                    TimeSpan.FromHours(4));
                logger.LogInformation("Jobs de monitoreo programados para apuesta {BetId}", bet.Id);
            }

            return ApplicationResult<AporteResponse>.Success(new AporteResponse(
                bet.Id,
                party.Id,
                req.Monto,
                party.MontoAcumulado,
                Math.Max(0, montoRestante - req.Monto),
                party.EstaCompleto,
                bet.Status == BetStatus.Active,
                wallet.SaldoDisponible));
        }
        catch (DomainException ex)
        {
            await unitOfWork.RollbackTransactionAsync(cancellationToken);
            return ApplicationResult<AporteResponse>.Failure("DOMAIN_ERROR", ex.Message);
        }
        catch (Exception ex)
        {
            await unitOfWork.RollbackTransactionAsync(cancellationToken);
            return ApplicationResult<AporteResponse>.Failure("ERROR", "Error interno del servidor.");
        }
    }
}

