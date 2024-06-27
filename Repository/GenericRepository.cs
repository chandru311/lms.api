using lms.api.Data;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace lms.api.Repository
{
    public class GenericRepository<T> : IGenericRepository<T> where T : class
    {
        private readonly ApplicationDbContext _context;
        public GenericRepository(ApplicationDbContext dbContext)
        {
            _context = dbContext;
        }
        public async Task Create(T entity)
        {
            await _context.AddAsync(entity);
            await Save();
        }

        public async Task Delete(T entity)
        {
            _context.Remove(entity);
            await Save();
        }

        public async Task<List<T>> GetAll()
        {
            return await _context.Set<T>().ToListAsync();
        }

        public async Task<T> Get(long id)
        {
            return await _context.Set<T>().FindAsync(id);
        }

        public bool IsRecordExists(Expression<Func<T, bool>> condition)
        {
            var result = _context.Set<T>().Where(condition).Any();
            return result;
        }

        public async Task Save()
        {
            await _context.SaveChangesAsync();
        }

        public async Task Update(T entity)
        {
            _ = _context.Set<T>().Update(entity).Entity;
            await Save();
        }
        public async Task<T> GetByCondition(Expression<Func<T, bool>> condition)
        {
            return await _context.Set<T>().FirstOrDefaultAsync(condition);
        }
    }
}
