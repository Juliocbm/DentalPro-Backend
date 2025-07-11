using System.Linq.Expressions;

namespace DentalPro.Application.Interfaces.IRepositories;

public interface IGenericRepository<T> where T : class
{
    Task<T?> GetByIdAsync(Guid id);
    Task<IEnumerable<T>> GetAllAsync();
    Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate);
    Task<T> AddAsync(T entity);
    Task UpdateAsync(T entity);
    Task RemoveAsync(T entity);
    Task<int> SaveChangesAsync();
    Task<bool> ExistsAsync(Guid id);
    
    /// <summary>
    /// Obtiene el contexto de base de datos actual
    /// </summary>
    /// <returns>El contexto de base de datos</returns>
    object GetDbContext();
}
