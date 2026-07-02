using Skillock.Domain.Enums;

namespace Skillock.Application.DTOs;

public record MatchResultDto(string MatchId, MatchResult Resultado, DateTime FinalizadoEn, string? RawApiResponse);

