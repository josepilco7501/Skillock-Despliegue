namespace Skillock.Domain.DTOs.Requests;

public record RegisterRequest(string Username, string Email, string Password);
public record LoginRequest(string Email, string Password);
