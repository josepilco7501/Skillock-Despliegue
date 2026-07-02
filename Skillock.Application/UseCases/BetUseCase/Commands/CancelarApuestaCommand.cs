using MediatR;
using Skillock.Application.Common;
using Skillock.Domain.DTOs.Requests;
using Skillock.Application.Interfaces;
using Skillock.Domain.Common;
using Skillock.Domain.Enums;
using Skillock.Domain.Models;

namespace Skillock.Application.UseCases.BetUseCase.Commands;

public record CancelarApuestaCommand(CancelarApuestaRequest Request)
    : IRequest<ApplicationResult>;

public class CancelarApuestaCommandHandler(IUnitOfWork unitOfWork) : IRequestHandler<CancelarApuestaCommand, ApplicationResult>
{
    public async Task<ApplicationResult> Handle(CancelarApuestaCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var req = request.Request;
            var bet = await unitOfWork.Bets.GetWithPartiesAsync(req.BetId, cancellationToken);
            if (bet is null)
                return ApplicationResult.NotFound("Bet", req.BetId);

            if (!bet.EsTransicionValida(BetStatus.Cancelled))
                return ApplicationResult.Failure("INVALID_TRANSITION", "No se puede cancelar la apuesta en este estado.");

            // Guid.Empty = cancelación automática del sistema (Hangfire) — salta validación de líder
            if (req.SolicitanteId != Guid.Empty)
            {
                var esLider = bet.BetParties.Any(p => 
                    p.Members.Any(m => m.UserId == req.SolicitanteId && m.Role == PartyRole.Leader));
                if (!esLider)
                    return ApplicationResult.Forbidden();
            }

            await unitOfWork.BeginTransactionAsync(cancellationToken: cancellationToken);

            foreach (var party in bet.BetParties)
            {
                foreach (var member in party.Members.Where(m => m.AporteConfirmado))
                {
                    var wallet = await unitOfWork.Wallets.GetByUserIdWithLockAsync(member.UserId, cancellationToken);
                    if (wallet is null) continue;

                    wallet.LiberarFondos(member.MontoAportado);

                    var tx = new WalletTransaction
                    {
                        WalletId = wallet.Id,
                        Type = TransactionType.BetRefund,
                        Amount = member.MontoAportado,
                        BalanceAfter = wallet.SaldoDisponible,
                        BetId = bet.Id,
                        Description = $"Reembolso por cancelación. Motivo: {req.Motivo ?? "No especificado"}"
                    };

                    wallet.Transactions.Add(tx);
                    unitOfWork.Wallets.Update(wallet);
                }
            }

            bet.Status = BetStatus.Cancelled;
            await unitOfWork.SaveChangesAsync(cancellationToken);
            await unitOfWork.CommitTransactionAsync(cancellationToken);

            return ApplicationResult.Success();
        }
        catch (DomainException ex)
        {
            await unitOfWork.RollbackTransactionAsync(cancellationToken);
            return ApplicationResult.Failure("DOMAIN_ERROR", ex.Message);
        }
        catch (Exception ex)
        {
            await unitOfWork.RollbackTransactionAsync(cancellationToken);
            return ApplicationResult.Failure("ERROR", "Error interno del servidor.");
        }
    }
}

