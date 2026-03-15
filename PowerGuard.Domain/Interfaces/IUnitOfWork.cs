using Microsoft.EntityFrameworkCore.Storage;
using PowerGuard.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PowerGuard.Domain.Interfaces
{
    public interface IUnitOfWork
    {
        IBaseRepository<Factory> Factories { get; }
        IBaseRepository<Department> Departments { get; }
        IBaseRepository<LimitHistory> LimitHistories { get; }
        IBaseRepository<AISuggestion> AISuggestions { get; }
        IBaseRepository<ConsumptionLog> ConsumptionLogs { get; }
        IBaseRepository<Notification> Notifications { get; }
        IBaseRepository<Alert> Alerts { get; }
        IBaseRepository<RefreshToken> RefreshTokens { get; }
        IBaseRepository<UserOTP> UserOTPs { get; }
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
        Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default);
        Task<IExecutionStrategy> CreateExecutionStrategy(CancellationToken cancellationToken = default);

    }
}
