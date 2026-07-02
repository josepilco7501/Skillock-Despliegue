using Skillock.Domain.Common;
using Skillock.Domain.Enums;

namespace Skillock.Domain.Models;

public class GameAccount(Guid userId, EsportGame game, string gamePlayerId, string gamePlayerTag, bool isActive, DateTime? verificadoEn) : BaseEntity
{
    public GameAccount() : this(Guid.Empty, EsportGame.Dota2, string.Empty, string.Empty, false, null) { }

    public Guid UserId { get; set; } = userId;
    public EsportGame Game { get; set; } = game;
    public string GamePlayerId { get; set; } = gamePlayerId;
    public string GamePlayerTag { get; set; } = gamePlayerTag;
    public bool IsActive { get; set; } = isActive;
    public DateTime? VerificadoEn { get; set; } = verificadoEn;

    // Navegación
    public virtual User? User { get; set; }
}

