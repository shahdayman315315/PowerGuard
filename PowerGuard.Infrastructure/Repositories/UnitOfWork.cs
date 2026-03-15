using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using PowerGuard.Domain.Interfaces;
using PowerGuard.Domain.Models;
using PowerGuard.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PowerGuard.Infrastructure.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly AppDbContext _context;

        private Lazy<IBaseRepository<Factory>> _factories;
        private Lazy<IBaseRepository<Department>> _departments;
        private Lazy<IBaseRepository<LimitHistory>> _limitHistories;
        private Lazy<IBaseRepository<AISuggestion>> _aiSuggestions;
        private Lazy<IBaseRepository<ConsumptionLog>> _consumptionLogs;
        private Lazy<IBaseRepository<Notification>> _notifications;
        private Lazy<IBaseRepository<Alert>> _alerts;
        private Lazy<IBaseRepository<RefreshToken>> _refreshTokens;
        private Lazy<IBaseRepository<UserOTP>> _userOTPs;

        public UnitOfWork(AppDbContext context)
        {
            _context = context;
            _factories = CreateRepository<IBaseRepository<Factory>, BaseRepository<Factory>>();
            _departments = CreateRepository<IBaseRepository<Department>, BaseRepository<Department>>();
            _limitHistories = CreateRepository<IBaseRepository<LimitHistory>, BaseRepository<LimitHistory>>();
            _aiSuggestions = CreateRepository<IBaseRepository<AISuggestion>, BaseRepository<AISuggestion>>();
            _consumptionLogs = CreateRepository<IBaseRepository<ConsumptionLog>, BaseRepository<ConsumptionLog>>();
            _notifications = CreateRepository<IBaseRepository<Notification>, BaseRepository<Notification>>();
            _alerts = CreateRepository<IBaseRepository<Alert>, BaseRepository<Alert>>();
            _refreshTokens = CreateRepository<IBaseRepository<RefreshToken>, BaseRepository<RefreshToken>>();
            _userOTPs = CreateRepository<IBaseRepository<UserOTP>, BaseRepository<UserOTP>>();

        }
        private Lazy<T1> CreateRepository<T1, T2>() where T1 : class where T2 : class
        {
            return new Lazy<T1>(() => (T1)Activator.CreateInstance(typeof(T2), _context)!);
        }
        public IBaseRepository<Factory> Factories => _factories.Value;

        public IBaseRepository<Department> Departments => _departments.Value;

        public IBaseRepository<LimitHistory> LimitHistories => _limitHistories.Value;

        public IBaseRepository<AISuggestion> AISuggestions => _aiSuggestions.Value;

        public IBaseRepository<ConsumptionLog> ConsumptionLogs => _consumptionLogs.Value;

        public IBaseRepository<Notification> Notifications => _notifications.Value;

        public IBaseRepository<Alert> Alerts => _alerts.Value;

        public IBaseRepository<RefreshToken> RefreshTokens => _refreshTokens.Value;
        public IBaseRepository<UserOTP> UserOTPs => _userOTPs.Value;

        public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            return await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default)
        {
            return await _context.Database.BeginTransactionAsync(cancellationToken);
        }

        public async Task<IExecutionStrategy> CreateExecutionStrategy(CancellationToken cancellationToken = default)
        {
            return  _context.Database.CreateExecutionStrategy();
        }
    }
}
