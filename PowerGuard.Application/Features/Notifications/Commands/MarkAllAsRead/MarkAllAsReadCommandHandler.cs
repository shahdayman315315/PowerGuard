using MediatR;
using Microsoft.EntityFrameworkCore;
using PowerGuard.Application.Helpers;
using PowerGuard.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PowerGuard.Application.Features.Notifications.Commands.MarkAllAsRead
{
    public class MarkAllAsReadCommandHandler : IRequestHandler<MarkAllAsReadCommand, Result<bool>>
    {
        private readonly IUnitOfWork _unitOfWork;

        public MarkAllAsReadCommandHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public async Task<Result<bool>> Handle(MarkAllAsReadCommand request, CancellationToken cancellationToken)
        {
            var unreadNotifications = await _unitOfWork.Notifications.Query
                .Where(n => n.UserId == request.userId && !n.IsRead)
                .ToListAsync(cancellationToken);

            foreach (var notification in unreadNotifications)
            {
                notification.IsRead = true;
            }

            var result = await _unitOfWork.SaveChangesAsync(cancellationToken);

            if (result >= 0)
            {
                return Result<bool>.Success(true);
            }

            return Result<bool>.Failure("Can't update notifications in the data base");
        }
    }
}
