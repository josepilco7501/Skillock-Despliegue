using GamerBet.Domain.Common;

namespace GamerBet.Domain.Entities;

/// <summary>
/// Usuario registrado en la plataforma. Posee exactamente una Wallet (1:1).
/// 
/// 💡 RECOMENDACIÓN: El hash de contraseña y la lógica de autenticación
/// deben vivir en Infrastructure (ASP.NET Core Identity o similar), NO aquí.
/// Esta entidad solo contiene los datos de dominio puros.
/// </summary>
public class User : BaseEntity
{
    public required string Username { get; set; }
    public required string Email { get; set; }

    /// <summary>
    /// Almacena el hash generado por ASP.NET Core Identity / BCrypt.
    /// NUNCA almacenar contraseña en texto plano.
    /// </summary>
    public required string PasswordHash { get; set; }

    public string? AvatarUrl { get; set; }
    public bool IsActive { get; set; } = true;

    // --- Navegación ---
    public Wallet Wallet { get; set; } = null!;
    public ICollection<PartyMember> PartyMemberships { get; set; } = [];
}
