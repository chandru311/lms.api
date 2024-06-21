using System.Linq.Expressions;

namespace lms.api.Repository
{
    public interface IGenericRepository<T> where T : class
    {
        Task<List<T>> GetAll();
        Task<T> Get(long id);
        Task Create(T entity);
        Task Delete(T entity);
        Task Update(T entity);
        Task Save();
        bool IsRecordExists(Expression<Func<T, bool>> condition);
    }
}
