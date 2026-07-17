using AutoMapper;
using AutoMapper.QueryableExtensions;
using MediatR;
using PowerGuard.Application.Dtos;
using PowerGuard.Application.Helpers;
using PowerGuard.Domain.Interfaces;
using PowerGuard.Infrastructure.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PowerGuard.Application.Features.Notifications.Queries.GetUserNotifications
{
    public class GetUserNotificationsQueryHandler :
        IRequestHandler<GetUserNotificationsQuery, Result<PagedResult<NotificationDto>>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public GetUserNotificationsQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper=mapper;
        }
        public async Task<Result<PagedResult<NotificationDto>>> Handle(GetUserNotificationsQuery request, CancellationToken cancellationToken)
        {
            var notifications = await _unitOfWork.Notifications.Query.
                Where(n => n.UserId == request.userId).OrderByDescending(n => n.CreatedAt).
                ProjectTo<NotificationDto>(_mapper.ConfigurationProvider)
                .ToPagedResultAsync(request.pageNumber, request.pageSize);

            return Result<PagedResult<NotificationDto>>.Success(notifications);
        }
    }
}
