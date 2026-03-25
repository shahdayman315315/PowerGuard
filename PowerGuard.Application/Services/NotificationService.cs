using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using PowerGuard.Application.Dtos;
using PowerGuard.Application.Helpers;
using PowerGuard.Application.Interfaces;
using PowerGuard.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PowerGuard.Application.Services
{
    public class NotificationService : INotificationService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        public NotificationService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _mapper = mapper;
            _unitOfWork = unitOfWork;
        }
        public async Task<Result<int>> GetUnReadCountAsync(string userId)
        {
            var unreadNotifications=await _unitOfWork.Notifications.Query.CountAsync(n=>n.UserId == userId && !n.IsRead);
           
            return Result<int>.Success(unreadNotifications);
        }

        public async Task<Result<IEnumerable<NotificationDto>>> GetUserNotificationAsync(string userId)
        {
            var notifications = await _unitOfWork.Notifications.Query.
                Where(n => n.UserId == userId).OrderByDescending(n=>n.CreatedAt).ProjectTo<NotificationDto>(_mapper.ConfigurationProvider).ToListAsync();

            return Result<IEnumerable<NotificationDto>>.Success(notifications);
        }

        public async Task<Result<bool>> MarkAsReadAsync(int notificationId, string userId)
        {
            var notification=await _unitOfWork.Notifications.Query.FirstOrDefaultAsync(n=>n.Id==notificationId && n.UserId==userId);

            if(notification is null)
            {
                return Result<bool>.Failure("Notification not found");
            }

            if(notification.IsRead)
            {
                return Result<bool>.Success(true);
            }

            notification.IsRead=true;
            _unitOfWork.Notifications.Update(notification);

            var result = await _unitOfWork.SaveChangesAsync();

            if (result >= 0)
            {
                return Result<bool>.Success(true);
            }

            return Result<bool>.Failure("Can't update notification in the data base");
        }
    }
}
