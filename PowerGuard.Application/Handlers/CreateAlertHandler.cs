using MediatR;
using PowerGuard.Application.Events;
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
        public CreateAlertHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public async Task Handle(HighConsumptionDetectedEvent notification, CancellationToken cancellationToken)
        {
            var alert = new Alert
            {
                ConsumptionLogId = notification.LogId,
                Type = AlertType.LimitExceeded,
                Severity = notification.Status == "Critical" ? AlertSeverity.High : AlertSeverity.Low,
                CreatedAt = DateTime.UtcNow
            };

            await _unitOfWork.Alerts.AddAsync(alert);
            await _unitOfWork.SaveChangesAsync();

            Console.WriteLine($"[Database] Alert created for Log {notification.LogId}");
        }
    }
}
