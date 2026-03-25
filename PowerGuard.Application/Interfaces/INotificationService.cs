using PowerGuard.Application.Dtos;
using PowerGuard.Application.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PowerGuard.Application.Interfaces
{
    public interface INotificationService
    {
        Task<Result<IEnumerable<NotificationDto>>> GetUserNotificationAsync(string userId);
        Task<Result<bool>> MarkAsReadAsync(int notificationId,string userId);
        Task<Result<int>> GetUnReadCountAsync(string userId);

    }
}
