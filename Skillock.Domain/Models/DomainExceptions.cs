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
