using lms.api.Data;
using lms.api.Models.RequestModels;
using lms.api.Models.ResponseModels;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using System.Reflection;

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

        public PaginationResponse<IQueryable<T>> GetByPagination(PaginationRequest paginationRequest, Expression<Func<T, bool>> expression)
        {
            PaginationResponse<IQueryable<T>> response = new();
            try
            {
                var data = _context.Set<T>();
                IQueryable<T> result = data;
                if (paginationRequest != null)
                {
                    if (paginationRequest.FilterCoulmn != null && paginationRequest.FilterCoulmn.Length != 0)
                    {
                        var expr = ApplyFilter(result, paginationRequest.FilterCoulmn, null);
                        if (expression != null)
                        {
                            var parameter = expr.Parameters[0];
                            var body = Expression.AndAlso(expr.Body, Expression.Invoke(expression, parameter));
                            expression = Expression.Lambda<Func<T, bool>>(body, parameter);
                        }
                        else
                        {
                            expression = expr;
                        }
                    }
                    if (expression != null)
                    {
                        result = data.Where(expression);
                    }
                    if (paginationRequest.SortColumn != null)
                    {
                        if (!string.IsNullOrEmpty(paginationRequest.SortColumn.SortByColumn))
                        {
                            result = ApplySort(result, paginationRequest.SortColumn);
                        }
                    }
                    response.TotalRecords = result.Count();
                    var totalPage = (response.TotalRecords / paginationRequest.PageSize) + ((response.TotalRecords % paginationRequest.PageSize) > 0 ? 1 : 0);
                    response.TotalPage = (int)totalPage;
                    response.CurrentPage = paginationRequest.PageNumber.HasValue ? paginationRequest.PageNumber.Value : 1;

                    if (paginationRequest.PageSize > 0 && paginationRequest.PageNumber.HasValue)
                    {
                        response.Model = Paginate(result, paginationRequest.PageNumber.Value, (int)paginationRequest.PageSize);
                    }

                }
                response.Success = true;
            }
            catch (Exception ex)
            {
                response.Message = ex.Message;
            }
            return response;
        }

        private IQueryable<T> ApplySort(IQueryable<T> data, SortColumn sortColumn)
        {
            var type = typeof(T);
            var property = type.GetProperty(sortColumn.SortByColumn);

            if (property == null)
                throw new ArgumentException($"Column '{sortColumn.SortByColumn}' does not exist in type '{type.Name}'.");

            var parameter = Expression.Parameter(type, "p");
            var propertyAccess = Expression.MakeMemberAccess(parameter, property);
            var orderByExp = Expression.Lambda(propertyAccess, parameter);

            var orderDirection = sortColumn.SortOrder == SortOrder.Ascending ? "OrderBy" : "OrderByDescending";
            var resultExp = Expression.Call(
                typeof(Queryable), orderDirection,
                new[] { type, property.PropertyType },
                data.Expression, Expression.Quote(orderByExp)
            );

            return data.Provider.CreateQuery<T>(resultExp);
        }

        private IQueryable<T> Paginate(IQueryable<T> data, int page, int pageSize)
        {
            return data.Skip((page - 1) * pageSize).Take(pageSize);
        }

        public Expression<Func<T, bool>> ApplyFilter(IQueryable<T> data, FilterCoulmn[] filterCoulmns, Expression<Func<T, bool>> condition)
        {
            if (filterCoulmns.Any())
            {
                for (int i = 0; i < filterCoulmns.Length; i++)
                {
                    var column = filterCoulmns[i];
                    var expr = BuildExpression<T>(column.ColumnName, column.FilterType, column.ColumnValue);
                    if (expr == null)
                    {
                        condition = expr;
                    }
                    else
                    {
                        var parameter = expr.Parameters[0];
                        var body = Expression.OrElse(expr.Body, Expression.Invoke(condition, parameter));
                        condition = Expression.Lambda<Func<T, bool>>(body, parameter);
                    }
                }
            }
            return condition;
        }

        private Expression<Func<T, bool>> BuildExpression<T>(string columnName, FilterType fType, string value)
        {
            var parameter = Expression.Parameter(typeof(T), "x");
            var property = typeof(T).GetProperty(columnName, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);

            if (property == null)
                throw new ArgumentException($"Column '{columnName}' does not exist in the entity '{typeof(T).Name}'.", nameof(columnName));
            object propertyValue;

            if (property.PropertyType == typeof(int?) || property.PropertyType == typeof(Nullable<int>))
            {
                int number;
                if (int.TryParse(value as string, out number))
                {
                    propertyValue = number;
                }
                else
                {
                    throw new Exception("Value can not be converted to integer");
                }
            }
            else if (property.PropertyType == typeof(DateTime?) || property.PropertyType == typeof(Nullable<int>))
            {
                DateTime dateTime;
                if (DateTime.TryParse(value as string, out dateTime))
                {
                    propertyValue = dateTime;
                }
                else
                {
                    throw new Exception("Value can not be converted to integer");
                }
            }
            else
            {
                propertyValue = Convert.ChangeType(value, property.PropertyType);
            }
#nullable enable
            BinaryExpression? expression = null;
#nullable disable
            Expression left = Expression.Property(parameter, property);
            Expression right = Expression.Constant(propertyValue);

            switch (fType)
            {
                case FilterType.EqualTo:

                    if (property.PropertyType == typeof(int?) || property.PropertyType == typeof(DateTime?))
                    {
                        expression = Expression.AndAlso(
                            Expression.PropertyOrField(left, "HasValue"),
                            Expression.Equal(Expression.PropertyOrField(left, "Value"), right)
                        );
                    }
                    else
                    {
                        expression = Expression.Equal(left, right);
                    }
                    return Expression.Lambda<Func<T, bool>>(expression, parameter);

                case FilterType.NotEqualTo:
                    expression = Expression.NotEqual(Expression.Property(parameter, columnName), Expression.Constant(propertyValue));
                    return Expression.Lambda<Func<T, bool>>(expression, parameter);

                case FilterType.Like:
                    var containsMethodInfo = typeof(string).GetMethod("Contains", new[] { typeof(string) });
                    var body = Expression.Call(Expression.Property(parameter, columnName), containsMethodInfo, Expression.Constant(propertyValue));
                    return Expression.Lambda<Func<T, bool>>(body, parameter);

                default:
                    throw new ArgumentException($"Unsupported operator: {fType}");

            }

        }
    }
}
