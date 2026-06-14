using GamerBet.Application.DTOs.Requests;
using GamerBet.Application.DTOs.Responses;

namespace GamerBet.Application.Interfaces;

/// <summary>
/// Servicio de aplicación para la fase de fondeo (estado Funding).
/// Responsable de validar y procesar los aportes individuales,
/// actualizar wallets y detectar cuándo ambos equipos están completos
/// para transicionar la apuesta a Active.
///
/// 💡 TODAS las operaciones de este servicio deben ejecutarse dentro
///    de una transacción EF Core con BeginTransactionAsync.
///    El fondeo toca 3 agregados simultáneamente:
///    Wallet (reserva), BetParty (monto acumulado) y WalletTransaction (ledger).
/// </summary>
public interface IFundingService
{
    /// <summary>
    /// Procesa el aporte de un miembro a su equipo.
    ///
    /// Flujo interno:
    ///   1. Validar que la apuesta está en Funding.
    ///   2. Validar que el aporte no excede el monto restante del equipo.
    ///   3. Validar que el usuario tiene SaldoDisponible suficiente.
    ///   4. Llamar Wallet.ReservarFondos() → SaldoDisponible → SaldoRetenido.
    ///   5. Actualizar BetParty.MontoAcumulado.
    ///   6. Registrar WalletTransaction (BetContribution).
    ///   7. Si AmbosEquiposCompletos() → transicionar a Active + registrar MatchId.
    ///   8. Persistir en una sola transacción.
    /// </summary>
    Task<AporteResponse> ProcesarAporteAsync(
        Guid betId,
        AporteRequest request,
        Guid userId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Invita a un usuario a unirse a un equipo como miembro (Fondo Mutuo).
    /// El invitado queda registrado como PartyMember con AporteConfirmado = false.
    /// Solo el Líder del equipo puede invitar miembros.
    /// </summary>
    Task<BetResponse> InvitarMiembroAsync(
        Guid betId,
        InvitarMiembroRequest request,
        Guid liderInvitanteId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Estado actual del fondeo de ambos equipos.
    /// Útil para que el frontend muestre la barra de progreso en tiempo real.
    /// </summary>
    Task<FondeoEstadoResponse> GetEstadoFondeoAsync(
        Guid betId,
        CancellationToken cancellationToken = default);
}
