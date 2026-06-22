using Skillock.Application.DTOs.Requests;
using Skillock.Application.DTOs.Responses;

namespace Skillock.Application.Interfaces;

/// <summary>
/// Servicio de aplicación para el ciclo de vida de la apuesta.
/// Cubre desde la creación (Draft) hasta la activación (Active).
/// La liquidación (Completed) es responsabilidad de ILiquidationService.
///
/// 💡 PRINCIPIO: Cada método orquesta el flujo completo de su operación:
///    validar → cargar entidades → ejecutar lógica de dominio → persistir → retornar DTO.
///    Los controladores NUNCA llaman directamente a repositorios.
/// </summary>
public interface IBetService
{
    /// <summary>
    /// El Líder del TeamA crea la apuesta en estado Draft.
    /// Resultado: apuesta creada con TeamA inicializado, sin TeamB aún.
    /// </summary>
    Task<BetResponse> CrearApuestaAsync(
        CrearApuestaRequest request,
        Guid liderId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// El Líder del TeamB acepta la invitación y la apuesta pasa a Negotiating.
    /// En este paso se inicializa el TeamB con su líder.
    /// </summary>
    Task<BetResponse> UnirseComoRivalAsync(
        Guid betId,
        Guid liderRivalId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Cualquier líder propone o actualiza el monto acordado durante la negociación.
    /// La apuesta permanece en Negotiating hasta que AMBOS líderes confirmen el mismo monto.
    /// </summary>
    Task<BetResponse> ProponerMontoAsync(
        Guid betId,
        ProponerMontoRequest request,
        Guid liderId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// El líder rival confirma el monto propuesto por el otro equipo.
    /// Si ambos han confirmado el mismo monto → transición a Agreed.
    /// </summary>
    Task<BetResponse> ConfirmarMontoAsync(
        Guid betId,
        Guid liderId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// El líder de un equipo elige la modalidad de fondeo de su bando (Individual o Mutual).
    /// Solo disponible en estado Agreed, antes de iniciar el fondeo.
    /// Cuando ambos líderes eligen su modalidad → transición a Funding.
    /// </summary>
    Task<BetResponse> ElegirModalidadFondeoAsync(
        Guid betId,
        ElegirModalidadRequest request,
        Guid liderId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Cancela la apuesta desde cualquier estado previo a Active.
    /// Libera todos los fondos retenidos de vuelta a SaldoDisponible.
    /// </summary>
    Task<BetResponse> CancelarApuestaAsync(
        Guid betId,
        Guid solicitanteId,
        string? motivo = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Consulta el estado completo de una apuesta con sus parties y miembros.
    /// </summary>
    Task<BetResponse?> GetBetAsync(Guid betId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Historial paginado de apuestas de un usuario.
    /// </summary>
    Task<PagedResponse<BetResumenResponse>> GetHistorialAsync(
        Guid userId,
        PaginacionRequest paginacion,
        CancellationToken cancellationToken = default);
}
