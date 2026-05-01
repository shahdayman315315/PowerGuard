using MediatR;
using PowerGuard.Application.Helpers;
using PowerGuard.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PowerGuard.Application.Features.Consumption.Commands.DeleteConsumptionLog
{
    public class DeleteConsumptionLogCommandHandler : IRequestHandler<DeleteConsumptionLogCommand,Result<bool>>
    {
        private readonly IUnitOfWork _unitOfWork;

        public DeleteConsumptionLogCommandHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Result<bool>> Handle(DeleteConsumptionLogCommand request, CancellationToken cancellationToken)
        {
            var log = await _unitOfWork.ConsumptionLogs.GetByIdAsync(request.LogId, cancellationToken);

            if (log == null)
            {
                return Result<bool>.Failure("Log not found", 404);
            }

            _unitOfWork.ConsumptionLogs.Delete(log);
            var result = await _unitOfWork.SaveChangesAsync(cancellationToken);

            return result > 0
                ? Result<bool>.Success(true)
                : Result<bool>.Failure("Error deleting the log");
        }
    }
}
