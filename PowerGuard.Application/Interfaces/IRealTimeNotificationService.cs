using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PowerGuard.Application.Interfaces
{
    public interface IRealTimeNotificationService
    {
        Task SendAlertAsync(string userId, string message, string severity,int alertId);
    }
}
