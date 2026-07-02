namespace Skillock.Domain.Common;

/// <summary>
/// Entidad base con auditoría. Todas las entidades del dominio heredan de aquí.
/// Usamos Guid como PK para compatibilidad con entornos distribuidos y para
/// evitar la exposición de IDs secuenciales en la API (seguridad por oscuridad básica).
/// </summary>
public abstract class BaseEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }


    // RECOMENDACIÓN: En producción considera agregar CreatedBy / UpdatedBy
    // para auditoría completa, integrado con el ICurrentUserService de Application.
}

