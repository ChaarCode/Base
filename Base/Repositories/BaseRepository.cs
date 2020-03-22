using CharCode.Base.Abstraction;
using CharCode.Base.Classes;
using CharCode.Base.Extentions;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CharCode.Base.Repositories
{
    public class BaseRepository<T, TKey, TDbContext> : IBaseRepository<T, TKey>
        where T : class, IModel<TKey>, new()
        where TDbContext : DbContext
    {
        protected TDbContext Context { get; private set; }
        protected IQueryable<T> DbSet => GetObjects();
        public BaseRepository(TDbContext bringoDbContext)
        {
            Context = bringoDbContext;
        }

        protected virtual IQueryable<T> GetObjects()
        {
            return Context.Set<T>().AsQueryable();
        }

        public async Task<T> GetAsync(TKey id)
        {
            return await Task.Run<T>(() =>
            {
                if (!TryGet(id, out T entity))
                    throw new KeyNotFoundException();

                return entity;
            });
        }

        public virtual async Task<List<T>> GetAsync(PaginationConfig config)
        {
            var filteredResult = DbSet.Filter(config);
            var sortedResult = filteredResult.Sort(config.Order, config.SortColumn);
            var result = sortedResult.Paginate(config.Skip, config.Take);

            return await result.ToListAsync();
        }

        public virtual async Task<long> GetCountAsync(PaginationConfig config)
        {
            var filteredResult = DbSet.Filter(config);

            return await filteredResult.LongCountAsync();
        }

        protected virtual bool TryGet(TKey id, out T entity)
        {
            entity = DbSet.SingleOrDefault(i => i.Id.Equals(id));

            return entity != null;
        }

        public virtual async Task<T> InsertAsync(T entity)
        {
            var inserted = await Context.Set<T>().AddAsync(entity);
            await Context.SaveChangesAsync();

            return inserted.Entity;
        }

        public virtual async Task InsertAsync(IEnumerable<T> entities)
        {
            await Context.Set<T>().AddRangeAsync(entities);
            await Context.SaveChangesAsync();
        }

        public virtual async Task DeleteAsync(TKey id)
        {
            if (!TryGet(id, out T entity))
                throw new KeyNotFoundException();

            Context.Set<T>().Remove(entity);
            await Context.SaveChangesAsync();
        }

        public virtual async Task DeleteAsync(IEnumerable<TKey> ids)
        {
            Context.Set<T>().RemoveRange(ids.Select(id => new T() { Id = id }));
            await Context.SaveChangesAsync();
        }

        public virtual async Task UpdateAsync(TKey id, T entity)
        {
            if (entity is null || !id.Equals(entity.Id))
                throw new ArgumentException();

            Context.Entry(entity).State = EntityState.Modified;

            try
            {
                await Context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                if (!await Context.Set<T>().AnyAsync(i => i.Id.Equals(entity.Id)))
                    throw new KeyNotFoundException();
                else
                    throw ex;
            }
        }

        public virtual async Task UpdateAsync(IEnumerable<T> entities)
        {
            foreach (var entity in entities)
            {
                Context.Entry(entity).State = EntityState.Modified;
            }

            await Context.SaveChangesAsync();
        }
    }

    public class BaseRepository<T, TDbContext> : BaseRepository<T, long, TDbContext> 
        where T : class, IModel, new()
        where TDbContext : DbContext
    {
        public BaseRepository(TDbContext bringoDbContext) : base(bringoDbContext)
        {
        }
    }
}
