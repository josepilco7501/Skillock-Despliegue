using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Skillock.Application.Common;
using Skillock.Application.Interfaces;
using Skillock.Application.UseCases.WalletUseCase.Commands;
using Skillock.Domain.Common;

namespace Skillock_ProyectoFinal.Controllers;

[ApiController]
[Authorize]
[Route("api/wallet")]
public class WalletController(IMediator mediator, IUnitOfWork unitOfWork) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(WalletResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetByUserId(CancellationToken cancellationToken)
    {
        try
        {
            var userId = CurrentUserId();
            var wallet = await unitOfWork.Wallets.GetByUserIdAsync(userId, cancellationToken);

            var result = wallet is null
                ? ApplicationResult<WalletResponse>.NotFound("Wallet", userId)
                : ApplicationResult<WalletResponse>.Success(
                    new WalletResponse(wallet.UserId, wallet.SaldoDisponible, wallet.SaldoRetenido));

            return ToActionResult(result);
        }
        catch (DomainException ex)
        {
            return UnprocessableEntity(ex.Message);
        }
    }

    [HttpPost("deposito")]
    [ProducesResponseType(typeof(WalletResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Deposito([FromBody] WalletMontoRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var result = await mediator.Send(new DepositarCommand(CurrentUserId(), request.Monto), cancellationToken);
            return ToActionResult(result);
        }
        catch (DomainException ex)
        {
            return UnprocessableEntity(ex.Message);
        }
    }

    [HttpPost("retiro")]
    [ProducesResponseType(typeof(WalletResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Retiro([FromBody] WalletMontoRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var result = await mediator.Send(new RetirarCommand(CurrentUserId(), request.Monto), cancellationToken);
            return ToActionResult(result);
        }
        catch (DomainException ex)
        {
            return UnprocessableEntity(ex.Message);
        }
    }

    private Guid CurrentUserId()
        => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    private IActionResult ToActionResult<T>(ApplicationResult<T> result)
    {
        if (result.IsSuccess)
            return Ok(result.Value);

        return result.ErrorCode switch
        {
            "NOT_FOUND" => NotFound(result.ErrorMessage),
            "FORBIDDEN" => Forbid(),
            "DOMAIN_ERROR" => UnprocessableEntity(result.ErrorMessage),
            _ => BadRequest(result.ErrorMessage)
        };
    }
}

public sealed record WalletMontoRequest(decimal Monto);


