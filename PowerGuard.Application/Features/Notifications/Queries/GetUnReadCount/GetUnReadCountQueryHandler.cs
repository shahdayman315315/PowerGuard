using MediatR;
using Microsoft.EntityFrameworkCore;
using PowerGuard.Application.Helpers;
using PowerGuard.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PowerGuard.Application.Features.Notifications.Queries.GetUnReadCount
{
    public class GetUnReadCountQueryHandler : IRequestHandler<GetUnReadCountQuery, Result<int>>
    {
        private readonly IUnitOfWork _unitOfWork;

        public GetUnReadCountQueryHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public async Task<Result<int>> Handle(GetUnReadCountQuery request, CancellationToken cancellationToken)
        {
            var unreadNotifications = await _unitOfWork.Notifications.
                Query.CountAsync(n => n.UserId == request.userId && !n.IsRead,cancellationToken);

            return Result<int>.Success(unreadNotifications);
        }
    }
}
