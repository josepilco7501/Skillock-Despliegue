using MediatR;
using Skillock.Application.Common;
using Skillock.Application.Interfaces;
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

        var wallet = await unitOfWork.Wallets.GetByUserIdAsync(request.UserId, cancellationToken);
        if (wallet is null)
            return ApplicationResult<WalletResponse>.NotFound("Wallet", request.UserId);

        wallet.SaldoDisponible += request.Monto;
        wallet.Transactions.Add(new WalletTransaction
        {
            WalletId = wallet.Id,
            Type = TransactionType.Deposit,
            Amount = request.Monto,
            BalanceAfter = wallet.SaldoDisponible,
            Description = "Depósito"
        });

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return ApplicationResult<WalletResponse>.Success(
            new WalletResponse(wallet.UserId, wallet.SaldoDisponible, wallet.SaldoRetenido));
    }
}

