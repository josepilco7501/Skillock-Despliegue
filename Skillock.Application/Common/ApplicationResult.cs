namespace Skillock.Application.Common;

/// <summary>
/// Abstracción para obtener el usuario autenticado en el contexto de la request.
/// Implementado en WebApi (lee el JWT claim). Application solo conoce el contrato.
///
/// 💡 USO: Los servicios reciben UserId directamente como parámetro en lugar
///    de inyectar ICurrentUserService. Esto hace los servicios más testeables
///    (no necesitan mockear el contexto HTTP). El controlador resuelve el UserId
///    del JWT y lo pasa al servicio.
///
///    Ejemplo en controlador:
///      var userId = _currentUser.UserId;
///      var result = await _betService.CrearApuestaAsync(request, userId, ct);
/// </summary>
public interface ICurrentUserService
{
    Guid UserId { get; }
    string Username { get; }
    bool IsAuthenticated { get; }
}

/// <summary>
/// Wrapper de resultado para operaciones de Application.
/// Evita usar excepciones para flujos de negocio esperados (validaciones, not found).
/// Las excepciones del dominio (DomainException) sí se propagan hacia arriba.
///
/// 💡 CUÁNDO USAR Result vs Exception:
///   - Result: casos esperados que el cliente puede manejar (validación, recurso no encontrado).
///   - Exception: violaciones de invariantes del dominio (DomainException → 422).
///   - Exception: errores inesperados de infraestructura (→ 500).
/// </summary>
public class ApplicationResult<T>
{
    public bool IsSuccess { get; private init; }
    public T? Value { get; private init; }
    public string? ErrorCode { get; private init; }
    public string? ErrorMessage { get; private init; }

    public static ApplicationResult<T> Success(T value) =>
        new() { IsSuccess = true, Value = value };

    public static ApplicationResult<T> Failure(string errorCode, string message) =>
        new() { IsSuccess = false, ErrorCode = errorCode, ErrorMessage = message };

    /// <summary>Shorthand para recursos no encontrados.</summary>
    public static ApplicationResult<T> NotFound(string entidad, Guid id) =>
        Failure("NOT_FOUND", $"{entidad} con Id '{id}' no fue encontrado.");

    /// <summary>Shorthand para acceso no autorizado a un recurso.</summary>
    public static ApplicationResult<T> Forbidden(string mensaje = "No tienes permisos para esta operación.") =>
        Failure("FORBIDDEN", mensaje);
}

/// <summary>Versión sin valor de retorno para operaciones void.</summary>
public class ApplicationResult : ApplicationResult<object>
{
    public static ApplicationResult Ok() =>
        new() { };

    // Hereda Failure y NotFound de la clase base tipada.
}
