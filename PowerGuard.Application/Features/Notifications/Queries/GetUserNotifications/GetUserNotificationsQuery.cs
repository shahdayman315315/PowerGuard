using MediatR;
using PowerGuard.Application.Dtos;
using PowerGuard.Application.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PowerGuard.Application.Features.Notifications.Queries.GetUserNotifications
{
    public sealed record GetUserNotificationsQuery(string userId, int pageNumber, int pageSize)
        :IRequest<Result<PagedResult<NotificationDto>>>;
    
}
