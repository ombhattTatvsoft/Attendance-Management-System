using System.Linq.Expressions;

namespace DAL.IRepository;

public interface IBaseRepository<T> where T : class
{
    Task<List<T>> GetAllAsync(Expression<Func<T, bool>>? filter=null);
    Task<T?> GetAsync(Expression<Func<T, bool>>? filter=null);
    Task AddAsync(T entity);
    void Update(T entity);
    Task SaveAsync();
}
