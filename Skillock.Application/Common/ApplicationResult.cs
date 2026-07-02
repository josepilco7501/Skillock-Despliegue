namespace Skillock.Application.Common;

public class ApplicationResult
{
    public bool IsSuccess { get; init; }
    public string? ErrorCode { get; init; }
    public string? ErrorMessage { get; init; }

    public static ApplicationResult Success() => new() { IsSuccess = true };
    public static ApplicationResult Failure(string code, string message) =>
        new() { IsSuccess = false, ErrorCode = code, ErrorMessage = message };
    public static ApplicationResult NotFound(string entidad, Guid id) =>
        Failure("NOT_FOUND", $"{entidad} con id {id} no encontrado.");
    public static ApplicationResult Forbidden() =>
        Failure("FORBIDDEN", "No tiene permisos para realizar esta operación.");
}

public class ApplicationResult<T> : ApplicationResult
{
    public T? Value { get; init; }

    public static ApplicationResult<T> Success(T value) =>
        new() { IsSuccess = true, Value = value };
    public static new ApplicationResult<T> Failure(string code, string message) =>
        new() { IsSuccess = false, ErrorCode = code, ErrorMessage = message };
    public static new ApplicationResult<T> NotFound(string entidad, Guid id) =>
        new() { IsSuccess = false, ErrorCode = "NOT_FOUND", ErrorMessage = $"{entidad} con id {id} no encontrado." };
    public static new ApplicationResult<T> Forbidden() =>
        new() { IsSuccess = false, ErrorCode = "FORBIDDEN", ErrorMessage = "No tiene permisos para realizar esta operación." };
}

