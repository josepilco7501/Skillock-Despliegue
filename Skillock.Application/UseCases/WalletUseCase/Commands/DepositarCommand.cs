using MediatR;
using Skillock.Application.Common;
using Skillock.Application.Interfaces;
using Skillock.Domain.Common;
using Skillock.Domain.Enums;
using Skillock.Domain.Models;

namespace Skillock.Application.UseCases.WalletUseCase.Commands;

public sealed record WalletResponse(Guid UserId, decimal SaldoDisponible, decimal SaldoRetenido);

public sealed record DepositarCommand(Guid UserId, decimal Monto) : IRequest<ApplicationResult<WalletResponse>>;

public sealed class DepositarCommandHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<DepositarCommand, ApplicationResult<WalletResponse>>
{
    public async Task<ApplicationResult<WalletResponse>> Handle(DepositarCommand request, CancellationToken cancellationToken)
    {
        if (request.Monto <= 0 || request.Monto > 10000m)
            return ApplicationResult<WalletResponse>.Failure(
                "MONTO_INVALIDO",
                "El monto debe ser mayor a 0 y no exceder 10000.");

        await unitOfWork.BeginTransactionAsync(cancellationToken: cancellationToken);
        try
        {
            // Obtener wallet con bloqueo FOR UPDATE para evitar condiciones de carrera
            var wallet = await unitOfWork.Wallets.GetByUserIdWithLockAsync(request.UserId, cancellationToken);
            if (wallet is null)
            {
                await unitOfWork.RollbackTransactionAsync(cancellationToken);
                return ApplicationResult<WalletResponse>.NotFound("Wallet", request.UserId);
            }

             wallet.SaldoDisponible += request.Monto;
             
             var transaction = new WalletTransaction
             {
                 WalletId = wallet.Id,
                 Type = TransactionType.Deposit,
                 Amount = request.Monto,
                 BalanceAfter = wallet.SaldoDisponible,
                 Description = "Depósito"
             };
             unitOfWork.Wallets.Update(wallet);
             await unitOfWork.WalletTransactions.AddAsync(transaction, cancellationToken);
             
             await unitOfWork.SaveChangesAsync(cancellationToken);
            await unitOfWork.CommitTransactionAsync(cancellationToken);

            return ApplicationResult<WalletResponse>.Success(
                new WalletResponse(wallet.UserId, wallet.SaldoDisponible, wallet.SaldoRetenido));
        }
         catch (DomainException)
         {
             await unitOfWork.RollbackTransactionAsync(cancellationToken);
             throw;
         }
        catch
        {
            await unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }
}

