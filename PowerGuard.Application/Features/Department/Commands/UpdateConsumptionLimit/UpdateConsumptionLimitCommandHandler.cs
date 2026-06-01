using MediatR;
using Microsoft.AspNetCore.Http;
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

namespace PowerGuard.Application.Features.Department.Commands.UpdateConsumptionLimit
{
    public class UpdateConsumptionLimitCommandHandler :
        IRequestHandler<UpdateConsumptionLimitCommand, Result<bool>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMediator _mediator;
        private readonly IEnumerable<IConsumptionEvaluationStrategy> _strategies;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public UpdateConsumptionLimitCommandHandler(IUnitOfWork unitOfWork, IMediator mediator, IEnumerable<IConsumptionEvaluationStrategy> strategies, IHttpContextAccessor httpContextAccessor)
        {
            _unitOfWork = unitOfWork;
            _mediator = mediator;
            _strategies = strategies;
            _httpContextAccessor = httpContextAccessor;
        }
        public async Task<Result<bool>> Handle(UpdateConsumptionLimitCommand request, CancellationToken cancellationToken)
        {
            var factoryIdClaim = _httpContextAccessor.HttpContext?.User.FindFirst("FactoryId")?.Value;

            if (string.IsNullOrEmpty(factoryIdClaim) || !int.TryParse(factoryIdClaim, out int factoryId))
            {
                return Result<bool>.Failure("Factory ID claim is missing or invalid.", 400);
            }

            var factory = await _unitOfWork.Factories.GetByIdAsync(factoryId);

            if (factory is null)
            {
                return Result<bool>.Failure("Departmentnot found");

            }

            if (factory.CurrentConsumptionLimit < request.NewLimit)
            {
                return Result<bool>.Failure("Department Limit can't be greater tha factory limit");
            }

            var department = await _unitOfWork.Departments.Query.FirstOrDefaultAsync(d => d.Id == request.departmentId && d.FactoryId == factoryId);

            if (department is null)
            {
                return Result<bool>.Failure("Department not found", 404);
            }

            var lastReading = await _unitOfWork.ConsumptionLogs.Query.Where(d => d.DepartmentId == request.departmentId).OrderByDescending(l => l.CapturedAt).FirstOrDefaultAsync();

            var departmentStatus = ConsumptionStatus.Normal;

            if (lastReading != null)
            {
                foreach (var strategy in _strategies)
                {
                    var status = strategy.Evaluate(lastReading.ConsumptionValue, request.NewLimit);

                    if (status > departmentStatus)
                    {
                        departmentStatus = status;
                    }
                }

            }


            department.CurrentConsumptionLimit = request.NewLimit;

            var limitHistory = new LimitHistory
            {
                FactoryId = factoryId,
                DepartmentId = request.departmentId,
                LimitValue = request.NewLimit,
                CreatedAt = DateTime.UtcNow,
                ActiveFrom = DateTime.UtcNow,
                SetBy = request.userId
            };

            await _unitOfWork.LimitHistories.AddAsync(limitHistory,cancellationToken);
            var result = await _unitOfWork.SaveChangesAsync(cancellationToken);

            if (result <= 0)
            {
                return Result<bool>.Failure("Failed to update the consumption limit in the database.");

            }

            if (lastReading is not null && departmentStatus != ConsumptionStatus.Normal)
            {
                await _mediator.Publish(new HighConsumptionDetectedEvent(lastReading.ConsumptionValue, ConsumptionStatus.Normal, factoryId, departmentStatus, department.Name, request.departmentId, lastReading.Id));

            }

            return Result<bool>.Success(true);

        }
    }
}
