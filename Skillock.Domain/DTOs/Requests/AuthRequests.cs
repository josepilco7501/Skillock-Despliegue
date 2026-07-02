namespace Skillock.Domain.DTOs.Requests;

public class RegisterRequest
{
	public string Username { get; set; } = string.Empty;
	public string Email { get; set; } = string.Empty;
	public string Password { get; set; } = string.Empty;
	public string? Role { get; set; }
}

public record LoginRequest(string Email, string Password);
