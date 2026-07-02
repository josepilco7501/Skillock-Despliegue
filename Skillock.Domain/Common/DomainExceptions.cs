using Skillock.Domain.Enums;

namespace Skillock.Domain.Common;

/// <summary>
/// Excepción base para violaciones de reglas de negocio del dominio.
/// Permite al middleware de WebApi devolver un 422 Unprocessable Entity
/// en lugar de un 500 cuando se viola una regla de negocio conocida.
/// </summary>
public class DomainException(string message) : Exception(message);

/// <summary>
/// Se lanza cuando se intenta una transición de estado inválida en la máquina de estados de Bet.
/// </summary>
public class InvalidBetStateTransitionException(BetStatus current, BetStatus attempted)
    : DomainException($"Transición de estado inválida: no se puede pasar de '{current}' a '{attempted}'.");

/// <summary>
/// Se lanza cuando un aporte excedería el monto acordado del equipo.
/// Regla de negocio: sin excedentes en ninguna modalidad de fondeo.
/// </summary>
public class ExcesoDeAporteException(decimal montoIntentado, decimal montoRestante)
    : DomainException(
        $"El aporte de {montoIntentado:C} excede el monto restante de {montoRestante:C}. " +
        $"El sistema no admite excedentes ni reembolsos parciales.");

/// <summary>
/// Se lanza cuando un usuario intenta operar con saldo insuficiente.
/// </summary>
public class SaldoInsuficienteException(decimal disponible, decimal requerido)
    : DomainException($"Saldo disponible insuficiente. Disponible: {disponible:C}, requerido: {requerido:C}.");

/// <summary>
/// Se lanza cuando el usuario intenta registrar una GameAccount para un juego que ya tiene.
/// </summary>
public class GameAccountDuplicadaException(EsportGame game)
    : DomainException($"El usuario ya tiene registrada una GameAccount para el juego '{game}'.");

/// <summary>
/// Se lanza cuando la combinación de tamaños de equipos no es válida.
/// </summary>
public class CombinacionEquiposInvalidaException(int sizeA, int sizeB)
    : DomainException($"Combinación de equipos inválida: {sizeA}v{sizeB}.");

/// <summary>
/// Se lanza cuando la apuesta expiró y no pudo completarse.
/// </summary>
public class ApuestaExpiradaException(DateTime expiresAt)
    : DomainException($"La apuesta expiró en {expiresAt:u} y ha sido cancelada.");

/// <summary>
/// Se lanza cuando el match inició después de la creación de la apuesta (inconsistencia).
/// </summary>
public class MatchIniciadoAntesDeCracionException(DateTime matchStart, DateTime betCreated)
    : DomainException($"El match inició en {matchStart:u} antes de la creación de la apuesta ({betCreated:u}).");

