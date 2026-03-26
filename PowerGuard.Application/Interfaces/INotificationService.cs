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
        Task<Result<PagedResult<NotificationDto>>> GetUserNotificationAsync(string userId, int pageNumber, int pageSize);
        Task<Result<bool>> MarkAsReadAsync(int notificationId,string userId);
        Task<Result<int>> GetUnReadCountAsync(string userId);
        Task<Result<bool>> MarkAllAsReadAsync(string userId);
        Task<Result<bool>> DeleteAll(string userId);
    }
}
