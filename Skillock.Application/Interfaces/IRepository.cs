using System.Linq.Expressions;
using GamerBet.Domain.Common;

namespace GamerBet.Application.Interfaces;

/// <summary>
/// Contrato genérico de repositorio. Deliberadamente minimalista:
/// solo operaciones que el dominio realmente necesita.
///
/// 💡 DECISIÓN DE DISEÑO: No exponemos IQueryable directamente.
/// Exponer IQueryable filtra la abstracción — el servicio puede construir
/// queries arbitrarias y la capa de Application se acopla a EF Core.
/// En su lugar, los repositorios concretos en Infrastructure expondrán
/// métodos con nombre para cada query compleja (ej: IBetRepository.GetActivasConParties).
/// </summary>
public interface IRepository<T> where T : BaseEntity
{
    Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<T>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<T>> FindAsync(
        Expression<Func<T, bool>> predicate,
        CancellationToken cancellationToken = default);
    Task AddAsync(T entity, CancellationToken cancellationToken = default);
    void Update(T entity);
    void Remove(T entity);
}
