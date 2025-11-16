namespace InvestCaixa.Domain.Interfaces;

/// <summary>
/// Repositório genérico para entidades com Id do tipo int
/// Usado para entidades como Cliente que usam int como chave primária
/// </summary>
public interface IRepositoryInt<T> where T : class
{
    Task<T?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<IEnumerable<T>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<T> AddAsync(T entity, CancellationToken cancellationToken = default);
    Task UpdateAsync(T entity, CancellationToken cancellationToken = default);
    Task DeleteAsync(int id, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(int id, CancellationToken cancellationToken = default);
}
