using Skillock.Domain.Common;
using Skillock.Domain.Enums;

namespace Skillock.Domain.Models;

public class GameAccount(Guid userId, EsportGame game, string gamePlayerId, string gamePlayerTag, bool isActive, DateTime? verificadoEn) : BaseEntity
{
    public GameAccount() : this(Guid.Empty, EsportGame.Dota2, string.Empty, string.Empty, false, null) { }

    public Guid UserId { get; init; } = userId;
    public EsportGame Game { get; init; } = game;
    public string GamePlayerId { get; init; } = gamePlayerId;
    public string GamePlayerTag { get; init; } = gamePlayerTag;
    public bool IsActive { get; set; } = isActive;
    public DateTime? VerificadoEn { get; set; } = verificadoEn;

    // Navegación
    public virtual User? User { get; set; }
}

