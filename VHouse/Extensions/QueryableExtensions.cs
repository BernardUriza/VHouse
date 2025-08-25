using Microsoft.EntityFrameworkCore;
using VHouse.Classes;

namespace VHouse.Extensions
{
    /// <summary>
    /// Extensions for IQueryable to support pagination and performance optimizations.
    /// </summary>
    public static class QueryableExtensions
    {
        /// <summary>
        /// Creates a paginated result from an IQueryable.
        /// </summary>
        public static async Task<PagedResult<T>> ToPagedResultAsync<T>(
            this IQueryable<T> query, 
            PaginationParameters pagination)
        {
            var totalItems = await query.CountAsync();

            var items = await query
                .Skip(pagination.Skip)
                .Take(pagination.Take)
                .ToListAsync();

            return new PagedResult<T>
            {
                Items = items,
                CurrentPage = pagination.Page,
                PageSize = pagination.PageSize,
                TotalItems = totalItems
            };
        }

        /// <summary>
        /// Creates a paginated result from an IQueryable with page and pageSize parameters.
        /// </summary>
        public static async Task<PagedResult<T>> ToPagedResultAsync<T>(
            this IQueryable<T> query,
            int page,
            int pageSize)
        {
            var totalItems = await query.CountAsync();

            var items = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PagedResult<T>
            {
                Items = items,
                CurrentPage = page,
                PageSize = pageSize,
                TotalItems = totalItems
            };
        }

        /// <summary>
        /// Creates a paginated result from an IQueryable with custom total count query.
        /// </summary>
        public static async Task<PagedResult<T>> ToPagedResultAsync<T>(
            this IQueryable<T> query,
            PaginationParameters pagination,
            IQueryable<T> countQuery)
        {
            var totalItems = await countQuery.CountAsync();

            var items = await query
                .Skip(pagination.Skip)
                .Take(pagination.Take)
                .ToListAsync();

            return new PagedResult<T>
            {
                Items = items,
                CurrentPage = pagination.Page,
                PageSize = pagination.PageSize,
                TotalItems = totalItems
            };
        }

        /// <summary>
        /// Applies search filter to a queryable based on a predicate.
        /// </summary>
        public static IQueryable<T> Search<T>(
            this IQueryable<T> query,
            string? searchTerm,
            System.Linq.Expressions.Expression<Func<T, bool>> searchPredicate)
        {
            if (!string.IsNullOrEmpty(searchTerm))
            {
                return query.Where(searchPredicate);
            }
            return query;
        }

        /// <summary>
        /// Applies sorting to a queryable using reflection.
        /// </summary>
        public static IQueryable<T> ApplySorting<T>(
            this IQueryable<T> query,
            string? sortBy,
            string sortDirection = "asc")
        {
            if (string.IsNullOrEmpty(sortBy))
                return query;

            var parameter = System.Linq.Expressions.Expression.Parameter(typeof(T));
            var property = typeof(T).GetProperty(sortBy, 
                System.Reflection.BindingFlags.IgnoreCase | 
                System.Reflection.BindingFlags.Public | 
                System.Reflection.BindingFlags.Instance);

            if (property == null)
                return query;

            var propertyAccess = System.Linq.Expressions.Expression.Property(parameter, property);
            var orderByExpression = System.Linq.Expressions.Expression.Lambda(propertyAccess, parameter);

            string methodName = sortDirection.ToLower() == "desc" ? "OrderByDescending" : "OrderBy";

            var resultExpression = System.Linq.Expressions.Expression.Call(
                typeof(Queryable),
                methodName,
                new Type[] { typeof(T), property.PropertyType },
                query.Expression,
                orderByExpression);

            return query.Provider.CreateQuery<T>(resultExpression);
        }

        /// <summary>
        /// Optimizes queries for read-only scenarios.
        /// </summary>
        public static IQueryable<T> AsReadOnly<T>(this IQueryable<T> query) where T : class
        {
            return query.AsNoTracking();
        }

        /// <summary>
        /// Applies caching to Entity Framework queries.
        /// </summary>
        public static IQueryable<T> WithCaching<T>(this IQueryable<T> query, TimeSpan? duration = null) where T : class
        {
            // Note: This would require a third-party library like EFCore.SecondLevelCacheInterceptor
            // For now, we'll return the query as-is
            return query;
        }
    }
}