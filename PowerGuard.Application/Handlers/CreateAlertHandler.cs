using MediatR;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using PowerGuard.Application.Events;
using PowerGuard.Application.Interfaces;
using PowerGuard.Domain.Enums;
using PowerGuard.Domain.Interfaces;
using PowerGuard.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PowerGuard.Application.Handlers
{
    public class CreateAlertHandler : INotificationHandler<HighConsumptionDetectedEvent>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IRealTimeNotificationService _rtNotificationService;       
        public CreateAlertHandler(IUnitOfWork unitOfWork, IRealTimeNotificationService rtNotificationService)
        {
            _unitOfWork = unitOfWork;
            _rtNotificationService=rtNotificationService;
        }
        public async Task Handle(HighConsumptionDetectedEvent notification, CancellationToken cancellationToken)
        {
            string dangStatus = notification.FactoryStatus > notification.DepartmentStatus ? notification.FactoryStatus.ToString() : notification.DepartmentStatus.ToString();

            var alert = new Alert
            {
                ConsumptionLogId = notification.LogId,
                Type = AlertType.LimitExceeded,
                Severity = dangStatus == "Exceeded" ? AlertSeverity.High : AlertSeverity.Low,
                CreatedAt = DateTime.UtcNow
            };


            await _unitOfWork.Alerts.AddAsync(alert);
            await _unitOfWork.SaveChangesAsync();

            var usersToNotify=new Dictionary<string,string>();

               var factory=await _unitOfWork.Factories.GetByIdAsync(notification.FactoryId);
                if (!string.IsNullOrEmpty(factory?.ManagerId))
                {
                    usersToNotify[factory.ManagerId] = "FactoryManager";
                }
            

            if(notification.DepartmentStatus != ConsumptionStatus.Normal)
            {
                var department=await _unitOfWork.Departments.GetByIdAsync(notification.DepartmentId);
                if (!string.IsNullOrEmpty(department?.ManagerId))
                {
                    usersToNotify[department.ManagerId]="DepartmentManager";
                }
            }

            foreach(var entry in usersToNotify)
            {
                var userId = entry.Key;
                var role = entry.Value;

                var message = role == "FactoryManager"
                    ? $"Factory limit exceeded due to {notification.DepartmentName} department!"
                    : $"Your department ({notification.DepartmentName}) consumption limit was exceeded!";

                var userNotification = new Notification
                {
                    Message = message,
                    UserId = userId,
                    AlertId = alert.Id
                };
                
                await _unitOfWork.Notifications.AddAsync(userNotification);

                try
                {
                    await _rtNotificationService.SendAlertAsync(userId, message,alert.Severity.ToString(),alert.Id);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"SignalR failed for user {userId}: {ex.Message}");
                }
            }

            
            await _unitOfWork.SaveChangesAsync();

            Console.WriteLine($"[Database] Alert created for Log {notification.LogId}");
        }
    }
}
