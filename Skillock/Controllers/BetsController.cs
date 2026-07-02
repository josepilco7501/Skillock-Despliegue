using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Skillock.Application.Common;
using Skillock.Application.Interfaces;
using Skillock.Application.UseCases.BetUseCase.Commands;
using Skillock.Application.UseCases.BetUseCase.Querys;
using Skillock.Application.UseCases.UserUseCase.Commands;
using Skillock.Domain.Common;
using Skillock.Domain.DTOs.Requests;
using Skillock.Domain.DTOs.Responses;

namespace Skillock_ProyectoFinal.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class BetsController(IMediator mediator, IReportsService reportsService, Skillock.Application.Interfaces.IUnitOfWork unitOfWork) : ControllerBase
{
    [HttpPost]
    [ProducesResponseType(typeof(BetResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Apuesta([FromBody] CrearApuestaRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var userId = CurrentUserId();
            var result = await mediator.Send(new CrearApuestaCommand(request, userId), cancellationToken);
            return ToActionResult(result);
        }
        catch (DomainException ex)
        {
            return UnprocessableEntity(ex.Message);
        }
    }

    [HttpPost("{codigoApuesta}/rival")]
    [ProducesResponseType(typeof(BetResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Rival([FromRoute] string codigoApuesta, [FromBody] UnirseComoRivalRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var result = await mediator.Send(new UnirseComoRivalCommand(codigoApuesta, request, CurrentUserId()), cancellationToken);
            return ToActionResult(result);
        }
        catch (DomainException ex)
        {
            return UnprocessableEntity(ex.Message);
        }
    }

    [HttpPut("{codigoApuesta}/monto")]
    [ProducesResponseType(typeof(BetResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Monto([FromRoute] string codigoApuesta, [FromBody] ProponerMontoRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var result = await mediator.Send(new ProponerMontoCommand(codigoApuesta, request.MontoPerTeam, CurrentUserId()), cancellationToken);
            return ToActionResult(result);
        }
        catch (DomainException ex)
        {
            return UnprocessableEntity(ex.Message);
        }
    }

    [HttpPut("{codigoApuesta}/monto/confirmacion")]
    [ProducesResponseType(typeof(BetResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Confirmacion([FromRoute] string codigoApuesta, CancellationToken cancellationToken)
    {
        try
        {
            var result = await mediator.Send(new ConfirmarMontoCommand(codigoApuesta, CurrentUserId()), cancellationToken);
            return ToActionResult(result);
        }
        catch (DomainException ex)
        {
            return UnprocessableEntity(ex.Message);
        }
    }

    [HttpPut("{codigoApuesta}/modalidad")]
    [ProducesResponseType(typeof(BetResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Modalidad([FromRoute] string codigoApuesta, [FromBody] ElegirModalidadRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var result = await mediator.Send(new ElegirModalidadFondeoCommand(codigoApuesta, request.Modalidad, CurrentUserId()), cancellationToken);
            return ToActionResult(result);
        }
        catch (DomainException ex)
        {
            return UnprocessableEntity(ex.Message);
        }
    }

    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Apuesta([FromRoute] Guid id, [FromQuery] string? motivo, CancellationToken cancellationToken)
    {
        try
        {
            var result = await mediator.Send(new CancelarApuestaCommand(new CancelarApuestaRequest(id, CurrentUserId(), motivo)), cancellationToken);
            return ToActionResult(result);
        }
        catch (DomainException ex)
        {
            return UnprocessableEntity(ex.Message);
        }
    }

    [HttpPost("{id}/fallo")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Fallo([FromRoute] Guid id, [FromBody] ReportarFalloRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var result = await mediator.Send(new ReportarFalloCommand(request with { BetId = id, SolicitanteId = CurrentUserId() }), cancellationToken);
            return ToActionResult(result);
        }
        catch (DomainException ex)
        {
            return UnprocessableEntity(ex.Message);
        }
    }

    [HttpGet("{id}")]
    [ProducesResponseType(typeof(BetResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Apuesta([FromRoute] Guid id, CancellationToken cancellationToken)
    {
        try
        {
            var result = await mediator.Send(new GetBetByIdQuery(id, CurrentUserId()), cancellationToken);
            return ToActionResult(result);
        }
        catch (DomainException ex)
        {
            return UnprocessableEntity(ex.Message);
        }
    }

    [HttpGet]
    [ProducesResponseType(typeof(PagedResponse<BetResumenResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Historial([FromQuery] int pagina = 1, [FromQuery] int tamano = 20, CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await mediator.Send(new GetHistorialQuery(CurrentUserId(), new PaginacionRequest(pagina, tamano)), cancellationToken);
            return ToActionResult(result);
        }
        catch (DomainException ex)
        {
            return UnprocessableEntity(ex.Message);
        }
    }

    [HttpGet("completed")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(PagedResponse<BetResumenResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Completed([FromQuery] int pagina = 1, [FromQuery] int tamano = 20, CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await mediator.Send(new GetCompletedBetsQuery(new PaginacionRequest(pagina, tamano)), cancellationToken);
            return ToActionResult(result);
        }
        catch (DomainException ex)
        {
            return UnprocessableEntity(ex.Message);
        }
    }

    [HttpGet("completed/report")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> CompletedReport(CancellationToken cancellationToken = default)
    {
        try
        {
            var pdf = await reportsService.GenerateCompletedBetsReportAsync(cancellationToken);
            return File(pdf, "application/pdf", "completed-bets-report.pdf");
        }
        catch (DomainException ex)
        {
            return UnprocessableEntity(ex.Message);
        }
    }

    // Endpoint para listar apuestas activas (solo Admin)
    [HttpGet("active")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(IEnumerable<Skillock.Domain.DTOs.Responses.BetResumenResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Active([FromQuery] int pagina = 1, [FromQuery] int tamano = 50, CancellationToken cancellationToken = default)
    {
        try
        {
            var (bets, total) = await unitOfWork.Bets.GetByStatusAsync(Skillock.Domain.Enums.BetStatus.Active, pagina, tamano, cancellationToken);

            var resumenes = bets.Select(bet => new Skillock.Domain.DTOs.Responses.BetResumenResponse(
                bet.Id,
                bet.Game,
                bet.Status,
                bet.AgreedAmountPerTeam,
                bet.PremioNeto,
                bet.MatchId,
                bet.CreatedAt,
                bet.CompletedAt,
                bet.TeamB?.Members.FirstOrDefault(m => m.Role == Skillock.Domain.Enums.PartyRole.Leader)?.User?.Username
            )).ToList().AsReadOnly();

            return Ok(resumenes);
        }
        catch (DomainException ex)
        {
            return UnprocessableEntity(ex.Message);
        }
    }

    // Endpoint para que Admin liquide (finalice) una apuesta activa
    [HttpPost("{id}/liquidar")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(Skillock.Domain.DTOs.Responses.LiquidacionResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> Liquidar([FromRoute] Guid id, [FromBody] Skillock.Domain.DTOs.Requests.LiquidarApuestaRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var result = await mediator.Send(new Skillock.Application.UseCases.BetUseCase.Commands.LiquidarApuestaCommand(id, request.Resultado), cancellationToken);
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

    private IActionResult ToActionResult(ApplicationResult result)
    {
        if (result.IsSuccess)
            return Ok();

        return result.ErrorCode switch
        {
            "NOT_FOUND" => NotFound(result.ErrorMessage),
            "FORBIDDEN" => Forbid(),
            "DOMAIN_ERROR" => UnprocessableEntity(result.ErrorMessage),
            _ => BadRequest(result.ErrorMessage)
        };
    }
}

