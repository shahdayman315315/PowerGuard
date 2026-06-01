using MediatR;
using Microsoft.EntityFrameworkCore;
using PowerGuard.Application.Events;
using PowerGuard.Application.Helpers;
using PowerGuard.Application.Interfaces;
using PowerGuard.Domain.Enums;
using PowerGuard.Domain.Interfaces;
using PowerGuard.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PowerGuard.Application.Features.Factory.UpdateConsumptionLimit
{
    public class UpdateConsumptionLimitCommandHandler : IRequestHandler<UpdateConsumptionLimitCommand, Result<bool>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMediator _mediator;
        private readonly IEnumerable<IConsumptionEvaluationStrategy> _strategies;

        public UpdateConsumptionLimitCommandHandler(IUnitOfWork unitOfWork, IMediator mediator, IEnumerable<IConsumptionEvaluationStrategy> strategies)
        {
            _unitOfWork = unitOfWork;
            _mediator = mediator;
            _strategies = strategies;
        }
        public async Task<Result<bool>> Handle(UpdateConsumptionLimitCommand request, CancellationToken cancellationToken)
        {
            var factory = await _unitOfWork.Factories.GetByIdAsync(request.factoryId,cancellationToken);

            if (factory is null)
            {
                return Result<bool>.Failure("Factory not found.", 404);
            }

            var departmentConsumptions = await _unitOfWork.Departments.Query
                 .Where(d => d.FactoryId == request.factoryId)
                 .Select(d => d.ConsumptionLogs
                 .OrderByDescending(log => log.CapturedAt)
                 .Select(log => log.ConsumptionValue)
                 .FirstOrDefault())
                 .ToListAsync(cancellationToken); 

            var currentTotalFactoryConsumption = departmentConsumptions.Sum();

            var factoryStatus = ConsumptionStatus.Normal;
            if (factory.CurrentConsumptionLimit.HasValue)
            {
                foreach (var strategy in _strategies)
                {
                    var status = strategy.Evaluate(currentTotalFactoryConsumption, request.NewLimit);

                    if (status > factoryStatus)
                    {
                        factoryStatus = status;
                    }
                }
            }


            if (factoryStatus != ConsumptionStatus.Normal)
            {
                await _mediator.Publish(new HighConsumptionDetectedEvent(currentTotalFactoryConsumption, factoryStatus, request.factoryId));
            }

            factory.CurrentConsumptionLimit = request.NewLimit;

            var limitHistory = new LimitHistory
            {
                FactoryId = request.factoryId,
                LimitValue = (decimal)factory.CurrentConsumptionLimit,
                CreatedAt = DateTime.UtcNow,
                ActiveFrom = DateTime.UtcNow,
                SetBy = request.userId
            };

            await _unitOfWork.LimitHistories.AddAsync(limitHistory,cancellationToken);
            var result = await _unitOfWork.SaveChangesAsync(cancellationToken);

            if (result > 0)
            {
                return Result<bool>.Success(true);
            }

            return Result<bool>.Failure("Failed to update the consumption limit in the database.");
        }
    }
}
