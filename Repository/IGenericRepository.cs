using lms.api.Models.RequestModels;
using lms.api.Models.ResponseModels;
using System.Linq.Expressions;

namespace lms.api.Repository
{
    public interface IGenericRepository<T> where T : class
    {
        PaginationResponse<IQueryable<T>> GetByPagination(PaginationRequest paginationRequest, Expression<Func<T, bool>>? condition);
        Task<List<T>> GetAll();
        Task<T> Get(long id);
        Task Create(T entity);
        Task Delete(T entity);
        Task Update(T entity);
        Task Save();
        Task<List<T>> Find(Expression<Func<T, bool>> condition);
        bool IsRecordExists(Expression<Func<T, bool>> condition);
        Task<T> GetByCondition(Expression<Func<T, bool>> condition);
    }
}
