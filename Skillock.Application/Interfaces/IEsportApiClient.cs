using Skillock.Application.DTOs.Responses;
using Skillock.Domain.Enums;

namespace Skillock.Application.Interfaces;

/// <summary>
/// Contrato para los clientes HTTP de APIs de Esports de terceros.
/// Cada juego tendrá su propia implementación en Infrastructure
/// (Dota2ApiClient, CS2ApiClient, ValorantApiClient) pero todos
/// cumplirán este mismo contrato.
///
/// 💡 PATRÓN: Strategy + Factory.
///    En Infrastructure, un IEsportApiClientFactory seleccionará
///    la implementación correcta según EsportGame.
///    Application solo conoce este contrato; nunca los clientes concretos.
/// </summary>
public interface IEsportApiClient
{
    /// <summary>Juego al que pertenece esta implementación del cliente.</summary>
    EsportGame JuegoSoportado { get; }

    /// <summary>
    /// Verifica que el MatchId existe y corresponde al juego correcto.
    /// Llamado antes de activar la apuesta.
    /// </summary>
    Task<bool> ValidarMatchIdAsync(string matchId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Consulta el resultado de la partida.
    /// Retorna null si la partida aún no terminó.
    /// </summary>
    Task<MatchResultResponse?> ConsultarResultadoAsync(
        string matchId,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Factory que resuelve el cliente correcto según el juego.
/// Implementado en Infrastructure, registrado como servicio en DI.
/// </summary>
public interface IEsportApiClientFactory
{
    IEsportApiClient GetClient(EsportGame game);
}
