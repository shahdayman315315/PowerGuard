using Microsoft.EntityFrameworkCore;
using PowerGuard.Domain.Interfaces;
using PowerGuard.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PowerGuard.Infrastructure.Repositories
{
    public class BaseRepository<T> : IBaseRepository<T> where T : class
    {
        private readonly AppDbContext _context;
        private readonly DbSet<T> _dbSet;

        public BaseRepository(AppDbContext context)
        {
            _context = context;
            _dbSet = context.Set<T>();
        }
        public IQueryable<T> Query => _dbSet;

        public async Task<T> AddAsync(T item, CancellationToken cancellationToken = default)
        {
            await _dbSet.AddAsync(item);
            return item;
        }

        public async Task AddRangeAsync(IEnumerable<T> values, CancellationToken cancellationToken = default)
        {
            await _dbSet.AddRangeAsync(values, cancellationToken);
        }

        public void Delete(T item)
        {
            _dbSet.Remove(item);
        }

        public async Task<IEnumerable<T>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return await _dbSet.ToListAsync(cancellationToken);
        }

        public async Task<T?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            return await _dbSet.FindAsync(id, cancellationToken);
        }

        public void RemoveRange(IEnumerable<T> entities)
        {
            _dbSet.RemoveRange(entities);
        }

        public void Update(T item)
        {
            _dbSet.Update(item);
        }
    }
}

