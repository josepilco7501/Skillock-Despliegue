using Microsoft.EntityFrameworkCore;
using Skillock.Domain.Enums;
using Skillock.Domain.Interfaces;
using Skillock.Domain.Models;
using Skillock.Infrastructure.Context;

namespace Skillock.Infrastructure.Persistence.Repositories;

public class BetRepository(SkillockDbContext context) : Repository<Bet>(context), IBetRepository
{
    public async Task<Bet?> GetWithPartiesAsync(Guid betId, CancellationToken cancellationToken = default)
        => await Context.Bets
            .Include(b => b.BetParties)
                .ThenInclude(bp => bp.Members)
                    .ThenInclude(m => m.User)
            .Include(b => b.Transactions)
            .FirstOrDefaultAsync(b => b.Id == betId, cancellationToken);

    public async Task<Bet?> GetWithPartiesByMatchIdAsync(string matchId, CancellationToken cancellationToken = default)
        => await Context.Bets
            .Include(b => b.BetParties)
                .ThenInclude(bp => bp.Members)
                    .ThenInclude(m => m.User)
            .Include(b => b.Transactions)
            .FirstOrDefaultAsync(b => b.MatchId == matchId, cancellationToken);

    public async Task<IReadOnlyList<Bet>> GetActivasParaMonitoreoAsync(CancellationToken cancellationToken = default)
        => await Context.Bets
            .AsNoTracking()
            .Where(b => b.Status == BetStatus.Active && b.MatchId != null)
            .Include(b => b.BetParties)
                .ThenInclude(bp => bp.Members)
                    .ThenInclude(m => m.User)
            .ToListAsync(cancellationToken);

    public async Task<(IReadOnlyList<Bet> Items, int TotalCount)> GetByUsuarioAsync(Guid userId, int pagina, int tamano, CancellationToken cancellationToken = default)
    {
        var query = Context.Bets
            .AsNoTracking()
            .Where(b => b.BetParties.Any(bp => bp.Members.Any(m => m.UserId == userId)))
            .Include(b => b.BetParties)
                .ThenInclude(bp => bp.Members)
                    .ThenInclude(m => m.User)
            .OrderByDescending(b => b.CreatedAt);

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .Skip((Math.Max(pagina, 1) - 1) * Math.Max(tamano, 1))
            .Take(Math.Max(tamano, 1))
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public async Task<(IReadOnlyList<Bet> Items, int TotalCount)> GetByStatusAsync(BetStatus status, int pagina, int tamano, CancellationToken cancellationToken = default)
    {
        var query = Context.Bets
            .AsNoTracking()
            .Where(b => b.Status == status)
            .Include(b => b.BetParties)
                .ThenInclude(bp => bp.Members)
                    .ThenInclude(m => m.User)
            .OrderByDescending(b => b.CreatedAt);

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .Skip((Math.Max(pagina, 1) - 1) * Math.Max(tamano, 1))
            .Take(Math.Max(tamano, 1))
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public async Task<IReadOnlyList<Bet>> GetFundingExpiradasAsync(DateTime ahora, CancellationToken cancellationToken = default)
        => await Context.Bets
            .AsNoTracking()
            .Include(b => b.BetParties)
                .ThenInclude(bp => bp.Members)
                    .ThenInclude(m => m.User)
            .Where(b => (b.Status == BetStatus.Draft || b.Status == BetStatus.Negotiating || b.Status == BetStatus.Agreed || b.Status == BetStatus.Funding) && b.ExpiresAt < ahora)
            .ToListAsync(cancellationToken);

    public async Task<bool> UsuarioTieneApuestaActivaAsync(Guid userId, EsportGame game, CancellationToken cancellationToken = default)
        => await Context.Bets.AsNoTracking().AnyAsync(b =>
            b.Game == game &&
            (b.Status == BetStatus.Draft || b.Status == BetStatus.Negotiating || b.Status == BetStatus.Agreed || b.Status == BetStatus.Funding || b.Status == BetStatus.Active) &&
            b.BetParties.Any(bp => bp.Members.Any(m => m.UserId == userId)), cancellationToken);
}

