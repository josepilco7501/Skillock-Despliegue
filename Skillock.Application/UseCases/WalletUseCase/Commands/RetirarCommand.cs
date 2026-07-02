using MediatR;
using Skillock.Application.Common;
using Skillock.Application.Interfaces;

namespace Skillock.Application.UseCases.WalletUseCase.Commands;

public sealed record RetirarCommand(Guid UserId, decimal Monto) : IRequest<ApplicationResult<WalletResponse>>;

public sealed class RetirarCommandHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<RetirarCommand, ApplicationResult<WalletResponse>>
{
    public async Task<ApplicationResult<WalletResponse>> Handle(RetirarCommand request, CancellationToken cancellationToken)
    {
        if (request.Monto <= 0)
            return ApplicationResult<WalletResponse>.Failure(
                "MONTO_INVALIDO",
                "El monto debe ser mayor a 0.");

        var wallet = await unitOfWork.Wallets.GetByUserIdWithLockAsync(request.UserId, cancellationToken);
        if (wallet is null)
            return ApplicationResult<WalletResponse>.NotFound("Wallet", request.UserId);

        if (wallet.SaldoDisponible < request.Monto)
            return ApplicationResult<WalletResponse>.Failure(
                "SALDO_INSUFICIENTE",
                "El saldo disponible es insuficiente para realizar el retiro.");

        var transactionStarted = false;

        try
        {
            await unitOfWork.BeginTransactionAsync(cancellationToken: cancellationToken);
            transactionStarted = true;

            wallet.SaldoDisponible -= request.Monto;
            wallet.Transactions.Add(new Domain.Models.WalletTransaction
            {
                WalletId = wallet.Id,
                Type = Domain.Enums.TransactionType.Withdrawal,
                Amount = request.Monto,
                BalanceAfter = wallet.SaldoDisponible,
                Description = "Retiro"
            });

            await unitOfWork.SaveChangesAsync(cancellationToken);
            await unitOfWork.CommitTransactionAsync(cancellationToken);

            return ApplicationResult<WalletResponse>.Success(
                new WalletResponse(wallet.UserId, wallet.SaldoDisponible, wallet.SaldoRetenido));
        }
        catch
        {
            if (transactionStarted)
                await unitOfWork.RollbackTransactionAsync(cancellationToken);

            return ApplicationResult<WalletResponse>.Failure(
                "ERROR_RETIRO",
                "No fue posible completar el retiro.");
        }
    }
}

