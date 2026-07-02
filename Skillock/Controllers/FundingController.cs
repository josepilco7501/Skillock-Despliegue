using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Skillock.Application.Common;
using Skillock.Application.UseCases.BetUseCase.Commands;
using Skillock.Application.UseCases.BetUseCase.Querys;
using Skillock.Domain.Common;
using Skillock.Domain.DTOs.Requests;
using Skillock.Domain.DTOs.Responses;

namespace Skillock_ProyectoFinal.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class FundingController(IMediator mediator) : ControllerBase
{
    [HttpPost("aporte")]
    [ProducesResponseType(typeof(AporteResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Aporte([FromBody] AportarFondosRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var result = await mediator.Send(new AportarFondosCommand(request, CurrentUserId()), cancellationToken);
            return ToActionResult(result);
        }
        catch (DomainException ex)
        {
            return UnprocessableEntity(ex.Message);
        }
    }

    [HttpPost("invitacion")]
    [ProducesResponseType(typeof(BetResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Invitacion([FromBody] InvitarMiembroRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var result = await mediator.Send(new InvitarMiembroCommand(request with { LiderId = CurrentUserId() }), cancellationToken);
            return ToActionResult(result);
        }
        catch (DomainException ex)
        {
            return UnprocessableEntity(ex.Message);
        }
    }

    [HttpGet("{betId}/estado")]
    [ProducesResponseType(typeof(FondeoEstadoResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Estado([FromRoute] Guid betId, CancellationToken cancellationToken)
    {
        try
        {
            var result = await mediator.Send(new GetEstadoFondeoQuery(betId), cancellationToken);
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

