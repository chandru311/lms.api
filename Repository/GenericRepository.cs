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

        public async Task<List<T>> Find(Expression<Func<T, bool>> condition)
        {
            return await _context.Set<T>().Where(condition).ToListAsync();
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

        public async Task<long> GenerateUniqueAiIdAsync()
        {
            Random random = new Random();
            long newAiId;
            bool exists;

            do
            {
                newAiId = random.Next(1000, 10000);
                exists = await _context.Set<T>().AnyAsync(e => EF.Property<long>(e, "AiId") == newAiId);
            }
            while (exists);

            return newAiId;
        }
    }
}
