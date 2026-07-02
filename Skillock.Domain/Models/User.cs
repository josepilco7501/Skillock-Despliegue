using Skillock.Domain.Common;

namespace Skillock.Domain.Models;

/// <summary>
/// Usuario registrado en la plataforma. Posee exactamente una Wallet (1:1).
/// 
/// RECOMENDACIÓN: El hash de contraseña y la lógica de autenticación
/// deben vivir en Infrastructure (ASP.NET Core Identity o similar), NO aquí.
/// Esta entidad solo contiene los datos de dominio puros.
/// </summary>
public class User(string username, string email) : BaseEntity
{
    public User() : this(string.Empty, string.Empty) { }

    public required string Username { get; set; } = username;
    public required string Email { get; set; } = email;
    
    public string Role { get; set; } = "Player"; // "Admin" o "Player"

    /// <summary>
    /// Almacena el hash generado por ASP.NET Core Identity / BCrypt.
    /// NUNCA almacenar contraseña en texto plano.
    /// </summary>
    public string PasswordHash { get; set; } = string.Empty;

    public string? AvatarUrl { get; set; }
    public bool IsActive { get; set; } = true;

    // --- Navegación ---
    public virtual Wallet? Wallet { get; set; }
    public virtual ICollection<PartyMember> PartyMemberships { get; set; } = new List<PartyMember>();
    public virtual ICollection<GameAccount> GameAccounts { get; set; } = new List<GameAccount>();
}

