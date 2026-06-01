using MediatR;
using Microsoft.EntityFrameworkCore;
using PowerGuard.Application.Helpers;
using PowerGuard.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PowerGuard.Application.Features.Notifications.Commands.MarkAsRead
{
    public class MarkAsReadCommandHandler : IRequestHandler<MarkAsReadCommand, Result<bool>>
    {
        private readonly IUnitOfWork _unitOfWork;

        public MarkAsReadCommandHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public async Task<Result<bool>> Handle(MarkAsReadCommand request, CancellationToken cancellationToken)
        {
            var notification = await _unitOfWork.Notifications.Query.FirstOrDefaultAsync(n => n.Id == request.notificationId && n.UserId == request.userId,cancellationToken);

            if (notification is null)
            {
                return Result<bool>.Failure("Notification not found", 404);
            }

            if (notification.IsRead)
            {
                return Result<bool>.Success(true);
            }

            notification.IsRead = true;
            _unitOfWork.Notifications.Update(notification);

            var result = await _unitOfWork.SaveChangesAsync(cancellationToken);

            if (result >= 0)
            {
                return Result<bool>.Success(true);
            }

            return Result<bool>.Failure("Can't update notification in the data base");
        }
    }
}
