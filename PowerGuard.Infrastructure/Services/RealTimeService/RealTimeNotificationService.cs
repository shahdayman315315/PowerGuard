using Microsoft.AspNetCore.SignalR;
using PowerGuard.Application.Interfaces;
using PowerGuard.Domain.Models;
using PowerGuard.Infrastructure.RealTimeService.Hubs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PowerGuard.Infrastructure.RealTimeService
{
    public class RealTimeNotificationService : IRealTimeNotificationService
    {
        private readonly IHubContext<NotificationHub> _hubContext;
        public RealTimeNotificationService(IHubContext<NotificationHub> hubContext)
        {
            _hubContext = hubContext;
        }
        public async Task SendAlertAsync(string userId, string message, string severity,int alertId)
        {
            
                await _hubContext.Clients.User(userId).SendAsync("ReceiveNotification", new
                {
                    Message = message,
                    AlertSeverity = severity,
                    CreatedAt = DateTime.UtcNow,
                    AlertId= alertId
                });
            
        }
    }
}
