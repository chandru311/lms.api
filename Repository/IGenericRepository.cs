﻿using System.Linq.Expressions;

namespace lms.api.Repository
{
    public interface IGenericRepository<T> where T : class
    {
        Task<List<T>> GetAll();
        Task<T> Get(long id);
        Task<T> GetID(int id);
        Task Create(T entity);
        Task Delete(T entity);
        Task Update(T entity);
        Task Save();
        Task<List<T>> Find(Expression<Func<T, bool>> condition);
        bool IsRecordExists(Expression<Func<T, bool>> condition);
    }
}
