using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Clud.Api.Infrastructure.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace Clud.Api.Infrastructure.DataAccess
{
    public static class QueryableExtensions
    {
        public static async Task<T> SingleOrThrowNotFound<T>(
            this IQueryable<T> queryable, Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default)
        {
            var entity = await queryable.SingleOrDefaultAsync(predicate, cancellationToken);
            if (entity == null)
            {
                throw new NotFoundException($"{typeof(T).Name} not found");
            }

            return entity;
        }

        public static async Task<T> SingleOrThrowNotFound<T>(this IQueryable<T> queryable, CancellationToken cancellationToken = default)
        {
            var entity = await queryable.SingleOrDefaultAsync(cancellationToken);
            if (entity == null)
            {
                throw new NotFoundException($"{typeof(T).Name} not found");
            }

            return entity;
        }
    }
}
