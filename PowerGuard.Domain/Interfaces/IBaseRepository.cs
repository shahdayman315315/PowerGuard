using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PowerGuard.Domain.Interfaces
{
    public interface IBaseRepository<T> where T : class
    {
        IQueryable<T> Query { get; }
        Task<IEnumerable<T>> GetAllAsync(CancellationToken cancellationToken = default);

        Task<T?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

        Task<T> AddAsync(T item, CancellationToken cancellationToken = default);

        Task AddRangeAsync(IEnumerable<T> values, CancellationToken cancellationToken = default);
        void RemoveRange(IEnumerable<T> entities);

        void Update(T item);

        void Delete(T item);
    }
}
