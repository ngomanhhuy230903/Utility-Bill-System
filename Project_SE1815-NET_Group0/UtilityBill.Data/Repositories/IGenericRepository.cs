using System.Linq.Expressions;

namespace UtilityBill.Data.Repositories
{
    public interface IGenericRepository<T> where T : class
    {
        Task<IEnumerable<T>> GetAllAsync();
        Task<T?> GetByIdAsync(int id);
        Task AddAsync(T entity);
        void Update(T entity);
        void Delete(T entity);
        Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate);
    }
}