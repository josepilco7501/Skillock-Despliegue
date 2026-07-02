using Microsoft.EntityFrameworkCore;
using Skillock.Application.Interfaces;
using System.Linq.Expressions;
using Skillock.Domain.Interfaces;
using Skillock.Infrastructure.Context;

namespace Skillock.Infrastructure.Persistence.Repositories;

public class Repository<T>(SkillockDbContext context) : IRepository<T> where T : class
{
    protected readonly SkillockDbContext Context = context;

    public virtual async Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => await Context.Set<T>().FindAsync([id], cancellationToken);

    public virtual async Task<IReadOnlyList<T>> GetAllAsync(CancellationToken cancellationToken = default)
        => await Context.Set<T>().AsNoTracking().ToListAsync(cancellationToken);

    public virtual async Task<IReadOnlyList<T>> FindAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default)
        => await Context.Set<T>().AsNoTracking().Where(predicate).ToListAsync(cancellationToken);

    public virtual async Task AddAsync(T entity, CancellationToken cancellationToken = default)
        => await Context.Set<T>().AddAsync(entity, cancellationToken);

    public virtual void Update(T entity)
        => Context.Set<T>().Update(entity);

    public virtual void Remove(T entity)
        => Context.Set<T>().Remove(entity);
}

