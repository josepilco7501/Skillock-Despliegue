using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Skillock.Application.Common;
using Skillock.Application.Interfaces;
using Skillock.Application.Interfaces.Services;
using Skillock.Domain.DTOs.Requests;
using Skillock.Domain.DTOs.Responses;
using Skillock.Domain.Models;

namespace Skillock.Infrastructure.Services;

public class AuthService(IUnitOfWork unitOfWork, IConfiguration configuration) : IAuthService
{
    public async Task<ApplicationResult<AuthResponse>> LoginAsync(LoginRequest request, CancellationToken ct)
    {
        var user = await unitOfWork.Users.GetByEmailAsync(request.Email, ct);

        if (user is null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            return ApplicationResult<AuthResponse>.Failure("INVALID_CREDENTIALS", "Credenciales inválidas.");

        return ApplicationResult<AuthResponse>.Success(GenerateToken(user));
    }

    public async Task<ApplicationResult<AuthResponse>> RegisterAsync(RegisterRequest request, CancellationToken ct)
    {
        if (await unitOfWork.Users.AnyByEmailAsync(request.Email, ct))
            return ApplicationResult<AuthResponse>.Failure("EMAIL_EXISTS", "El email ya está registrado.");

        if (await unitOfWork.Users.AnyByUsernameAsync(request.Username, ct))
            return ApplicationResult<AuthResponse>.Failure("USERNAME_EXISTS", "El nombre de usuario ya está en uso.");

        var user = new User
        {
            Username = request.Username,
            Email = request.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            Role = "Player"
        };

        user.Wallet = new Wallet();

        await unitOfWork.Users.AddAsync(user, ct);
        await unitOfWork.SaveChangesAsync(ct);

        return ApplicationResult<AuthResponse>.Success(GenerateToken(user));
    }

    private AuthResponse GenerateToken(User user)
    {
        var expiresAt = DateTime.UtcNow.AddHours(24);
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["Jwt:Key"]!));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.Username),
            new Claim(ClaimTypes.Role, user.Role)
        };

        var token = new JwtSecurityToken(
            issuer: configuration["Jwt:Issuer"],
            audience: configuration["Jwt:Audience"],
            claims: claims,
            expires: expiresAt,
            signingCredentials: credentials);

        var tokenString = new JwtSecurityTokenHandler().WriteToken(token);

        return new AuthResponse(tokenString, user.Username, user.Role, expiresAt);
    }
}
