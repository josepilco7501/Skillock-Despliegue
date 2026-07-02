using Skillock.Domain.Enums;

namespace Skillock.Domain.DTOs.Responses;

public record GameAccountResponse(
    Guid Id,
    EsportGame Game,
    string GamePlayerId,
    string GamePlayerTag,
    bool IsActive,
    DateTime? VerificadoEn,
    DateTime CreatedAt);
