using Skillock.Application.Common;
using Skillock.Domain.DTOs.Requests;
using Skillock.Domain.DTOs.Responses;

namespace Skillock.Application.Interfaces.Services;

public interface IAuthService
{
    Task<ApplicationResult<AuthResponse>> RegisterAsync(RegisterRequest request, CancellationToken ct);
    Task<ApplicationResult<AuthResponse>> LoginAsync(LoginRequest request, CancellationToken ct);
    Task<bool> HasAnyAdminAsync(CancellationToken ct = default);
}
