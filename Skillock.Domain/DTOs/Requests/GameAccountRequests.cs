using Skillock.Domain.Enums;

namespace Skillock.Domain.DTOs.Requests;

public record VincularGameAccountRequest(EsportGame Game, string GamePlayerId, string GamePlayerTag);
