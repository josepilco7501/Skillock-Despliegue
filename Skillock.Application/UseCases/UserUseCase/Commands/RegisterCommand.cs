using MediatR;
using Skillock.Application.Common;
using Skillock.Application.Interfaces.Services;
using Skillock.Domain.DTOs.Requests;
using Skillock.Domain.DTOs.Responses;

namespace Skillock.Application.UseCases.UserUseCase.Commands;

public sealed record RegisterCommand(RegisterRequest Request) : IRequest<ApplicationResult<AuthResponse>>;

public sealed class RegisterCommandHandler(IAuthService authService)
    : IRequestHandler<RegisterCommand, ApplicationResult<AuthResponse>>
{
    public Task<ApplicationResult<AuthResponse>> Handle(RegisterCommand request, CancellationToken cancellationToken)
        => authService.RegisterAsync(request.Request, cancellationToken);
}

