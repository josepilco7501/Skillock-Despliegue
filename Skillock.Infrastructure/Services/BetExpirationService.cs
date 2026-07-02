using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Skillock.Application.UseCases.BetUseCase.Commands;
using Skillock.Application.Interfaces;
using Skillock.Domain.DTOs.Requests;

namespace Skillock.Infrastructure.Services;

public class BetExpirationService(IServiceScopeFactory scopeFactory, ILogger<BetExpirationService> logger) : IBetExpirationService
{
    public async Task EjecutarCicloExpiracionAsync(CancellationToken cancellationToken)
    {
        using var scope = scopeFactory.CreateScope();
        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

        var expiradas = await unitOfWork.Bets.GetFundingExpiradasAsync(DateTime.UtcNow, cancellationToken);

        foreach (var bet in expiradas)
        {
            try
            {
                await mediator.Send(new CancelarApuestaCommand(new CancelarApuestaRequest(bet.Id, Guid.Empty, "Expiración automática")), cancellationToken);
                logger.LogInformation("Apuesta {BetId} cancelada por expiración automática", bet.Id);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error cancelando apuesta {BetId} por expiración", bet.Id);
            }
        }
    }
}


