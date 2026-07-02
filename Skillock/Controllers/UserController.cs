using Microsoft.AspNetCore.Mvc;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Skillock.Application.UseCases.UserUseCase.Commands;
using Skillock.Application.Interfaces.Services;
using Skillock.Domain.DTOs.Requests;

namespace Skillock_ProyectoFinal.Controllers;

[ApiController]
[Route("api/auth")]
public class UserController(IMediator mediator, IAuthService authService) : ControllerBase
{
    
    [HttpPost("registro")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Registro([FromBody] RegisterRequest request, CancellationToken cancellationToken)
    {
        // Evitar que registros públicos se auto-asignen rol Admin.
        request.Role = null;

        var result = await mediator.Send(new RegisterCommand(request), cancellationToken);

        if (!result.IsSuccess)
        {
            return result.ErrorCode switch
            {
                "EMAIL_EXISTS" or "USERNAME_EXISTS" => Conflict(new { result.ErrorCode, result.ErrorMessage }),
                _ => BadRequest(new { result.ErrorCode, result.ErrorMessage })
            };
        }

        return Ok(result.Value);
    }

    [Authorize(Roles = "Admin")]
    [HttpPost("admin/registro")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> RegistroAdmin([FromBody] RegisterRequest request, CancellationToken cancellationToken)
    {
        // Forzar rol Admin para este endpoint protegido
        request.Role = "Admin";
        var result = await mediator.Send(new RegisterCommand(request), cancellationToken);

        if (!result.IsSuccess)
        {
            return result.ErrorCode switch
            {
                "EMAIL_EXISTS" or "USERNAME_EXISTS" => Conflict(new { result.ErrorCode, result.ErrorMessage }),
                _ => BadRequest(new { result.ErrorCode, result.ErrorMessage })
            };
        }

        return Ok(result.Value);
    }

    [HttpPost("acceso")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Acceso([FromBody] LoginRequest request, CancellationToken cancellationToken)
    {
        var result = await authService.LoginAsync(request, cancellationToken);

        if (!result.IsSuccess)
        {
            return result.ErrorCode switch
            {
                "INVALID_CREDENTIALS" => Unauthorized(new { result.ErrorCode, result.ErrorMessage }),
                _ => BadRequest(new { result.ErrorCode, result.ErrorMessage })
            };
        }

        return Ok(result.Value);
    }
}
