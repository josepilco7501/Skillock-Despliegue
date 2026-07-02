namespace Skillock.Domain.DTOs.Responses;

public record AuthResponse(string Token, string Username, string Role, DateTime ExpiresAt);
