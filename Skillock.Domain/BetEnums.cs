namespace GamerBet.Domain.Enums;

/// <summary>
/// Ciclo de vida completo de una apuesta PvP.
/// Los estados siguen una máquina de estados estricta; ninguna transición
/// arbitraria debe permitirse desde la capa de Application.
/// </summary>
public enum BetStatus
{
    /// <summary>El Líder creó la apuesta pero aún no invitó al equipo rival.</summary>
    Draft = 0,

    /// <summary>Ambos equipos están negociando el monto fijo por bando.</summary>
    Negotiating = 1,

    /// <summary>Ambos equipos aceptaron el monto fijo. Inicia la fase de fondeo.</summary>
    Agreed = 2,

    /// <summary>
    /// Al menos un equipo está completando su aporte.
    /// El dinero pasa de SaldoDisponible → SaldoRetenido al contribuir.
    /// </summary>
    Funding = 3,

    /// <summary>
    /// Ambos bandos completaron exactamente el monto acordado.
    /// El MatchID se envía al servicio de Esports para monitoreo.
    /// </summary>
    Active = 4,

    /// <summary>El BackgroundService confirmó el resultado. Premio distribuido.</summary>
    Completed = 5,

    /// <summary>
    /// Apuesta cancelada en cualquier fase previa a Active.
    /// SaldoRetenido se devuelve a SaldoDisponible para todos los contribuyentes.
    /// </summary>
    Cancelled = 6,

    /// <summary>
    /// Estado de falla: el MatchID no pudo validarse o expiró el tiempo de monitoreo.
    /// Requiere intervención manual o reembolso automático.
    /// </summary>
    Disputed = 7
}

/// <summary>
/// Modalidad de fondeo elegida por cada bando de forma independiente.
/// </summary>
public enum FundingMode
{
    /// <summary>El Líder del equipo aporta el 100% del monto acordado.</summary>
    Individual = 0,

    /// <summary>Los integrantes aportan libremente hasta completar exactamente el monto acordado.</summary>
    Mutual = 1
}

/// <summary>
/// Juegos de Esports soportados por la plataforma.
/// Usar enum en lugar de string libre previene datos sucios y facilita
/// el switch en los clientes HTTP de cada API de terceros.
/// </summary>
public enum EsportGame
{
    Dota2 = 0,
    CS2 = 1,
    Valorant = 2
}

/// <summary>
/// Resultado final de la partida, resuelto por el BackgroundService.
/// </summary>
public enum MatchResult
{
    Pending = 0,
    TeamAWins = 1,
    TeamBWins = 2,

    /// <summary>Empate técnico; no aplica en la mayoría de Esports 1v1 pero se reserva.</summary>
    Draw = 3
}

/// <summary>
/// Tipo de movimiento en el ledger de wallet.
/// Mantener un historial de transacciones inmutable es crítico para auditoría
/// y resolución de disputas. NUNCA eliminar registros de WalletTransaction.
/// </summary>
public enum TransactionType
{
    /// <summary>Depósito externo (pasarela de pago).</summary>
    Deposit = 0,

    /// <summary>Retiro a cuenta bancaria / cripto.</summary>
    Withdrawal = 1,

    /// <summary>Fondos movidos de Disponible → Retenido al contribuir a una apuesta.</summary>
    BetContribution = 2,

    /// <summary>Premio acreditado en SaldoDisponible al ganar.</summary>
    BetPayout = 3,

    /// <summary>Devolución de SaldoRetenido → Disponible al cancelar una apuesta.</summary>
    BetRefund = 4,

    /// <summary>Comisión de plataforma descontada del premio bruto.</summary>
    PlatformFee = 5
}

/// <summary>
/// Rol del usuario dentro de una party/equipo en una apuesta.
/// El Líder tiene privilegios adicionales: acepta el monto y elige la modalidad de fondeo.
/// </summary>
public enum PartyRole
{
    Leader = 0,
    Member = 1
}
