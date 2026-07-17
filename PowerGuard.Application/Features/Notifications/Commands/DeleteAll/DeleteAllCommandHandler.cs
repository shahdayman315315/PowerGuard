using MediatR;
using Microsoft.EntityFrameworkCore;
using PowerGuard.Application.Helpers;
using PowerGuard.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PowerGuard.Application.Features.Notifications.Commands.DeleteAll
{
    public class DeleteAllCommandHandler : IRequestHandler<DeleteAllCommand, Result<bool>>
    {
        private readonly IUnitOfWork _unitOfWork;

        public DeleteAllCommandHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public async Task<Result<bool>> Handle(DeleteAllCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var Notifications = await _unitOfWork.Notifications.Query
                               .Where(n => n.UserId == request.userId)
                               .ExecuteDeleteAsync(cancellationToken);

                return Result<bool>.Success(true);
            }

            catch (Exception ex)
            {
                return Result<bool>.Failure($"Can't delete notifications from the data base  {ex.Message}");
            }
        }
    }
}
