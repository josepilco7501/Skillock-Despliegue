using GamerBet.Application.DTOs.Responses;
using GamerBet.Domain.Enums;

namespace GamerBet.Application.Interfaces;

/// <summary>
/// Servicio de liquidación. Llamado exclusivamente por el BackgroundService
/// de monitoreo cuando se confirma el resultado de una partida.
///
/// 💡 NUNCA exponer este servicio directamente en un endpoint HTTP.
///    La liquidación debe ser disparada solo por el sistema interno.
/// </summary>
public interface ILiquidationService
{
    /// <summary>
    /// Distribuye el premio entre los miembros del equipo ganador
    /// y consume el saldo retenido del equipo perdedor.
    ///
    /// Flujo interno:
    ///   1. Cargar Bet con parties y miembros.
    ///   2. Calcular PremioNeto = PremioTotal * (1 - PlatformFeePercent).
    ///   3. Para cada miembro del equipo GANADOR:
    ///      - CalcularProporcionPremio() → Wallet.AcreditarPremio().
    ///      - Registrar WalletTransaction (BetPayout).
    ///   4. Para cada miembro del equipo PERDEDOR:
    ///      - Wallet.ConsumirRetenido().
    ///      - Registrar WalletTransaction (BetContribution ya consumida, sin nuevo registro).
    ///   5. Registrar WalletTransaction de PlatformFee en wallet de la plataforma.
    ///   6. Transicionar Bet a Completed.
    ///   7. Persistir en una sola transacción.
    /// </summary>
    Task<LiquidacionResponse> LiquidarApuestaAsync(
        Guid betId,
        MatchResult resultado,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Reembolsa todos los aportes cuando la apuesta no pudo resolverse
    /// (MatchId inválido, timeout de monitoreo, disputa).
    /// Transiciona a Cancelled o Disputed según el motivo.
    /// </summary>
    Task ReembolsarApuestaAsync(
        Guid betId,
        string motivo,
        CancellationToken cancellationToken = default);
}
