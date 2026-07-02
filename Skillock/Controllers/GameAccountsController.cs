using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Skillock.Application.Common;
using Skillock.Application.UseCases.GameAccountUseCase.Commands;
using Skillock.Application.UseCases.GameAccountUseCase.Querys;
using Skillock.Domain.Common;
using Skillock.Domain.DTOs.Requests;
using Skillock.Domain.DTOs.Responses;

namespace Skillock_ProyectoFinal.Controllers;

[ApiController]
[Authorize]
[Route("api/game-accounts")]
public class GameAccountsController(IMediator mediator) : ControllerBase
{
    [HttpPost]
    [ProducesResponseType(typeof(GameAccountResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Vinculacion(
        [FromBody] VincularGameAccountRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var result = await mediator.Send(
                new VincularGameAccountCommand(request, CurrentUserId()), cancellationToken);
            return ToActionResult(result);
        }
        catch (DomainException ex)
        {
            return UnprocessableEntity(ex.Message);
        }
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Vinculacion(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            var result = await mediator.Send(
                new DesvincularGameAccountCommand(id, CurrentUserId()), cancellationToken);
            return ToDeleteActionResult(result);
        }
        catch (DomainException ex)
        {
            return UnprocessableEntity(ex.Message);
        }
    }

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<GameAccountResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Cuentas(CancellationToken cancellationToken)
    {
        try
        {
            var result = await mediator.Send(new GetMisGameAccountsQuery(CurrentUserId()), cancellationToken);
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
            "GAME_ACCOUNT_EXISTS" or "MAX_ACCOUNTS_REACHED" or "ACCOUNT_IN_USE" => Conflict(result.ErrorMessage),
            "INVALID_PLAYER_ID" => UnprocessableEntity(result.ErrorMessage),
            "DOMAIN_ERROR" => UnprocessableEntity(result.ErrorMessage),
            _ => BadRequest(result.ErrorMessage)
        };
    }

    private IActionResult ToDeleteActionResult(ApplicationResult result)
    {
        if (result.IsSuccess)
            return NoContent();

        return result.ErrorCode switch
        {
            "NOT_FOUND" => NotFound(result.ErrorMessage),
            "FORBIDDEN" => Forbid(),
            "ACCOUNT_IN_USE" => Conflict(result.ErrorMessage),
            "DOMAIN_ERROR" => UnprocessableEntity(result.ErrorMessage),
            _ => BadRequest(result.ErrorMessage)
        };
    }
}
