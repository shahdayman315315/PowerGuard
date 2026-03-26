using AutoMapper;
using AutoMapper.QueryableExtensions;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using Org.BouncyCastle.Math.EC;
using PowerGuard.Application.Dtos;
using PowerGuard.Application.Events;
using PowerGuard.Application.Helpers;
using PowerGuard.Application.Interfaces;
using PowerGuard.Domain.Enums;
using PowerGuard.Domain.Interfaces;
using PowerGuard.Domain.Models;
using PowerGuard.Infrastructure.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;

namespace PowerGuard.Application.Services
{
    public class ConsumptionService : IConsumptionService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IEnumerable<IConsumptionEvaluationStrategy> _strategies;
        private readonly IMediator _mediator;
        private readonly IMapper _mapper;
        public ConsumptionService(IHttpContextAccessor httpContextAccessor, IUnitOfWork unitOfWork, 
            IEnumerable<IConsumptionEvaluationStrategy> strategies, IMediator mediator,IMapper mapper)
        {
            _httpContextAccessor = httpContextAccessor;
            _unitOfWork = unitOfWork;
            _strategies = strategies;
            _mediator = mediator;
            _mapper = mapper;
        }
      
        public async Task<Result<ConsumptionLogDto>> EnterConsumptionAsync(ConsumptionLogDto dto)
        {
            var factoryIdClaim = _httpContextAccessor.HttpContext?.User.FindFirst("FactoryId")?.Value;

            if (string.IsNullOrEmpty(factoryIdClaim) || !int.TryParse(factoryIdClaim, out int factoryId))
            {
                return Result<ConsumptionLogDto>.Failure("Factory ID claim is missing or invalid.", 400);
            }

            var factory = await _unitOfWork.Factories.GetByIdAsync(factoryId);

            if(factory is null)
            {
                return Result<ConsumptionLogDto>.Failure("Factory not found", 404);

            }

            var department = _unitOfWork.Departments.Query.FirstOrDefault(d => d.Id == dto.DepartmentId && d.FactoryId == factoryId);

            if (department is null)
            {
                return Result<ConsumptionLogDto>.Failure("department not found");
            }

            var date=DateTime.UtcNow.Date;

            var previousDayTotalFactoryConsumption = await _unitOfWork.Departments.Query
                .AsNoTracking()
                .Where(d => d.FactoryId == factoryId)
                .Select(d => d.ConsumptionLogs.Where(l => l.CapturedAt < date)
                    .OrderByDescending(log => log.CapturedAt)
                    .Select(log => log.ConsumptionValue)
                    .FirstOrDefault()) // بياخد آخر قيمة للقسم ده، لو مفيش هتبقى 0
                .SumAsync();

            var currentTotalFactoryConsumption = await _unitOfWork.Departments.Query
                .Where(d => d.FactoryId == factoryId && d.Id !=department.Id)
                .Select(d => d.ConsumptionLogs.Where(l=>l.CapturedAt>=date)
                    .OrderByDescending(log => log.CapturedAt)
                    .Select(log => log.ConsumptionValue)
                    .FirstOrDefault()) // بياخد آخر قيمة للقسم ده، لو مفيش هتبقى 0
                .SumAsync();

            var newTotalFactoryConsumption = currentTotalFactoryConsumption + dto.ConsumptionValue;

            var factoryConsumptionForToday = Math.Max(0, newTotalFactoryConsumption - previousDayTotalFactoryConsumption);

            var previousDayDepartmentConsumption = await _unitOfWork.ConsumptionLogs.Query
                .AsNoTracking()
                .Where(l => l.DepartmentId == department.Id && l.CapturedAt < date)
                .OrderByDescending(l => l.CapturedAt)
                .FirstOrDefaultAsync();

            
            var departmentConsumptionForToday = Math.Max(0, dto.ConsumptionValue - (previousDayDepartmentConsumption == null ? 0 : previousDayDepartmentConsumption.ConsumptionValue));

            var finalDepartmentStatus = ConsumptionStatus.Normal;
            if (department.CurrentConsumptionLimit.HasValue)
            {
                foreach (var strategy in _strategies)
                {
                    var status = strategy.Evaluate(departmentConsumptionForToday, department.CurrentConsumptionLimit ?? 0);

                    if (status > finalDepartmentStatus)
                    {
                        finalDepartmentStatus = status;
                    }
                }

            }

            var finalFactoryStatus= ConsumptionStatus.Normal;
            if (factory.CurrentConsumptionLimit.HasValue)
            {
                foreach (var strategy in _strategies)
                {
                    var status = strategy.Evaluate(factoryConsumptionForToday, factory.CurrentConsumptionLimit ?? 0);

                    if (status > finalFactoryStatus)
                    {
                        finalFactoryStatus = status;
                    }
                }
            }
            

            var log = new ConsumptionLog
            {
                ConsumptionValue = dto.ConsumptionValue,
                DepartmentId = dto.DepartmentId,
                Status = finalDepartmentStatus,
                CapturedAt = dto.CapturedAt
            };

            await _unitOfWork.ConsumptionLogs.AddAsync(log);
            var result = await _unitOfWork.SaveChangesAsync();

            if (result <= 0)
            {
                return Result<ConsumptionLogDto>.Failure("Error Adding The log in the data base");
            }

            if(finalDepartmentStatus != ConsumptionStatus.Normal ||finalFactoryStatus !=ConsumptionStatus.Normal)
            {
                await _mediator.Publish(new HighConsumptionDetectedEvent( departmentConsumptionForToday, finalFactoryStatus, factoryId, finalDepartmentStatus, department.Name,department.Id, log.Id));
            }

            var logDto=_mapper.Map<ConsumptionLogDto>(log);    
            return Result<ConsumptionLogDto>.Success(logDto);
        }

        public async Task<Result<IEnumerable<DepartmentDailyConsumptionSummaryDto>>> GetAllDepartmentsSummaryAsync(int factoryId)
        {
            var factory = await _unitOfWork.Factories.Query.Include(f=>f.Departments).FirstOrDefaultAsync(f=>f.Id==factoryId);

            if (factory is null)
            {
                return Result<IEnumerable<DepartmentDailyConsumptionSummaryDto>>.Failure("Factory not found",404);
            }

            List<DepartmentDailyConsumptionSummaryDto> summaryDtos=new List<DepartmentDailyConsumptionSummaryDto>();

            var departments = factory.Departments;

            if (!departments.Any())
            {
                return Result<IEnumerable<DepartmentDailyConsumptionSummaryDto>>.Failure("No departments found for this factory", 404);
            }

            foreach (var department in departments)
            {
                var result = await GetDepartmentDailySummaryAsync(department.Id);

                if (result.IsSuccess)
                {
                    summaryDtos.Add(result.Data!);
                }
            }

            var sortedSummaries = summaryDtos
                  .OrderByDescending(s => s.Status == "Critical")
                  .ThenByDescending(s => s.Status == "Warning")
                  .ThenBy(s => s.DepartmentName);

            return Result<IEnumerable<DepartmentDailyConsumptionSummaryDto>>.Success(sortedSummaries);
        }

        public async Task<Result<PagedResult<ConsumptionLogDto>>> GetConsumptionLogsAsync(int departmentId, int pageNumber, int pageSize)
        {
            var department=await _unitOfWork.Departments.GetByIdAsync(departmentId);

            if(department is null)
            {
                return Result<PagedResult<ConsumptionLogDto>>.Failure("Department not found", 404);
            }

            var logs =  _unitOfWork.ConsumptionLogs.Query.AsNoTracking().Where(l => l.DepartmentId == departmentId)
                .OrderByDescending(l => l.CapturedAt);

            var pagedData = await logs.ProjectTo<ConsumptionLogDto>(_mapper.ConfigurationProvider)
                .ToPagedResultAsync(pageNumber, pageSize);

            return Result<PagedResult<ConsumptionLogDto>>.Success(pagedData);
        }

        public async Task<Result<DepartmentDailyConsumptionSummaryDto>> GetDepartmentDailySummaryAsync(int departmentId)
        {
            var department = await _unitOfWork.Departments.GetByIdAsync(departmentId);
            if (department is null)
            {
                return Result<DepartmentDailyConsumptionSummaryDto>.Failure("Department not found", 404);
            }

            var date = DateTime.UtcNow.Date;

            var latestLog = await _unitOfWork.ConsumptionLogs.Query.AsNoTracking()
                .Where(l => l.DepartmentId == departmentId && l.CapturedAt >= date).OrderByDescending(l => l.CapturedAt)
                .FirstOrDefaultAsync();

            var latestLogBeforeToday = await _unitOfWork.ConsumptionLogs.Query.AsNoTracking()
                .Where(l => l.DepartmentId == departmentId && l.CapturedAt < date).OrderByDescending(l => l.CapturedAt)
                .FirstOrDefaultAsync();

            var latestLogBeforeYesterday = await _unitOfWork.ConsumptionLogs.Query.AsNoTracking()
                .Where(l => l.DepartmentId == departmentId && l.CapturedAt < date.AddDays(-1)).OrderByDescending(l => l.CapturedAt)
                .FirstOrDefaultAsync();

            var actualConsumptionForToday = Math.Max(0,(latestLog==null?0: latestLog.ConsumptionValue) -( latestLogBeforeToday==null?0: latestLogBeforeToday.ConsumptionValue));

            var currentLimit = department.CurrentConsumptionLimit;

            var percentage = currentLimit==null?0:(double)(actualConsumptionForToday / currentLimit) * 100;

            var remainingAmount = currentLimit - actualConsumptionForToday;

            var totalConsumptionYesterday =Math.Max(0,((latestLogBeforeToday==null?0: latestLogBeforeToday.ConsumptionValue )- (latestLogBeforeYesterday==null?0: latestLogBeforeYesterday.ConsumptionValue)));

            double consumptionDiffAmount = 0;
            if (totalConsumptionYesterday > 0)
            {
                consumptionDiffAmount = (double)((actualConsumptionForToday - totalConsumptionYesterday) / totalConsumptionYesterday) * 100;
            }

            var finalStatus = ConsumptionStatus.Normal;
            foreach (var strategy in _strategies)
            {
                var status = strategy.Evaluate(actualConsumptionForToday, currentLimit ?? 0);

                if (status > finalStatus)
                {
                    finalStatus = status;
                }
            }

            var summaryDto = new DepartmentDailyConsumptionSummaryDto
            {
                DepartmentName = department.Name,
                TotalConsumption = Math.Round(actualConsumptionForToday,2),
                CurrentLimit = currentLimit ?? 0,
                ConsumptionPercentage = Math.Round(percentage,1),
                RemainingAmount = Math.Round(remainingAmount ?? 0,2),
                Status = finalStatus.ToString(),
                LastReadingValue = latestLog==null?0: latestLog.ConsumptionValue,
                LastReadingAt = latestLog==null?null: latestLog.CapturedAt,
                ComparisonWithYesterday =(double) Math.Round(consumptionDiffAmount,2)
            };

            return Result<DepartmentDailyConsumptionSummaryDto>.Success(summaryDto);
        }

        public async Task<Result<FactoryDailySummaryDto>> GetFactoryDailySummaryAsync(int factoryId)
        {
            var factory=await _unitOfWork.Factories.GetByIdAsync(factoryId);

            if(factory is null)
            {
                return Result<FactoryDailySummaryDto>.Failure("Factory not found",404);
            }

            var date = DateTime.UtcNow.Date;

            var latestConsumption = await _unitOfWork.Departments.Query
                .AsNoTracking().Where(d => d.FactoryId == factoryId)
                .Select(d => d.ConsumptionLogs
                .Where(l => l.CapturedAt >= date)
                .OrderByDescending(l=>l.CapturedAt)
                .Select(l => l.ConsumptionValue)
                .FirstOrDefault())
                .SumAsync();

            var latestConsumptionBeforeToday = await _unitOfWork.Departments.Query
                .AsNoTracking().Where(d => d.FactoryId == factoryId)
                .Select(d => d.ConsumptionLogs
                .Where(l => l.CapturedAt < date)
                .OrderByDescending(l => l.CapturedAt)
                .Select(l => l.ConsumptionValue)
                .FirstOrDefault())
                .SumAsync();

            var latestConsumptionBeforeYesterday = await _unitOfWork.Departments.Query
               .AsNoTracking().Where(d => d.FactoryId == factoryId)
               .Select(d => d.ConsumptionLogs
               .Where(l => l.CapturedAt < date.AddDays(-1))
               .OrderByDescending(l => l.CapturedAt)
               .Select(l => l.ConsumptionValue)
               .FirstOrDefault())
               .SumAsync();

            var factoryConsumptionForToday= Math.Max(0, latestConsumption - latestConsumptionBeforeToday);

            var currentLimit = factory.CurrentConsumptionLimit ?? 0;

            var percentage = currentLimit == 0 ? 0 : (factoryConsumptionForToday / currentLimit) * 100;

            var remainingAmount= currentLimit == 0 ? 0 : currentLimit - factoryConsumptionForToday;

            var factoryConsumptionForYesterday= Math.Max(0, latestConsumptionBeforeToday - latestConsumptionBeforeYesterday);

            double consumptionDiffAmount = 0;
            if (factoryConsumptionForYesterday > 0)
            {
                consumptionDiffAmount = (double)((factoryConsumptionForToday - factoryConsumptionForYesterday) / factoryConsumptionForYesterday) * 100;
            }

            var finalStatus=ConsumptionStatus.Normal;
            foreach (var strategy in _strategies)
            {
                var status = strategy.Evaluate(factoryConsumptionForToday, currentLimit );

                if (status > finalStatus)
                {
                    finalStatus = status;
                }
            }

            var summaryDto = new FactoryDailySummaryDto
            {
                FactoryName = factory.Name,
                TotalConsumption = Math.Round(factoryConsumptionForToday),
                ConsumptionPercentage = (double)Math.Round(percentage,1),
                CurrentLimit = currentLimit,
                RemainingAmount = Math.Round(remainingAmount,2),
                Status = finalStatus.ToString(),
                ComparisonWithYesterday = Math.Round(consumptionDiffAmount,2)
            };


            return Result<FactoryDailySummaryDto>.Success(summaryDto);
        }

        public async Task<Result<ConsumptionLogDto>> UpdateConsumptionLogAsync(UpdateConsumptionLogDto dto)
        {
            var log=await _unitOfWork.ConsumptionLogs.Query.Include(l=>l.Department).FirstOrDefaultAsync(l=>l.Id==dto.LogId);

            if(log is null)
            {
                return Result<ConsumptionLogDto>.Failure("Log is not found");
            }

            log.ConsumptionValue = dto.ConsumptionValue;

            var factoryIdClaim = _httpContextAccessor.HttpContext?.User.FindFirst("FactoryId")?.Value;

            if (string.IsNullOrEmpty(factoryIdClaim) || !int.TryParse(factoryIdClaim, out int factoryId))
            {
                return Result<ConsumptionLogDto>.Failure("Factory ID claim is missing or invalid.", 400);
            }

            var factory = await _unitOfWork.Factories.GetByIdAsync(factoryId);

            if (factory is null)
            {
                return Result<ConsumptionLogDto>.Failure("Factory not found", 404);

            }


            var date = log.CapturedAt.Date;

            var previousDayTotalFactoryConsumption = await _unitOfWork.Departments.Query
                .AsNoTracking()
                .Where(d => d.FactoryId == factoryId)
                .Select(d => d.ConsumptionLogs.Where(l => l.CapturedAt < date)
                    .OrderByDescending(log => log.CapturedAt)
                    .Select(log => log.ConsumptionValue)
                    .FirstOrDefault()) // بياخد آخر قيمة للقسم ده، لو مفيش هتبقى 0
                .SumAsync();

            var currentTotalFactoryConsumption = await _unitOfWork.Departments.Query
                .Where(d => d.FactoryId == factoryId && d.Id != log.DepartmentId)
                .Select(d => d.ConsumptionLogs.Where(l => l.CapturedAt >= date)
                    .OrderByDescending(log => log.CapturedAt)
                    .Select(log => log.ConsumptionValue)
                    .FirstOrDefault()) // بياخد آخر قيمة للقسم ده، لو مفيش هتبقى 0
                .SumAsync();

            var newTotalFactoryConsumption = currentTotalFactoryConsumption + dto.ConsumptionValue;

            var factoryConsumptionForToday = Math.Max(0, newTotalFactoryConsumption - previousDayTotalFactoryConsumption);

            var previousDayDepartmentConsumption = await _unitOfWork.ConsumptionLogs.Query
                .AsNoTracking()
                .Where(l => l.DepartmentId == log.DepartmentId && l.CapturedAt < date)
                .OrderByDescending(l => l.CapturedAt)
                .FirstOrDefaultAsync();


            var departmentConsumptionForToday = Math.Max(0, dto.ConsumptionValue - (previousDayDepartmentConsumption == null ? 0 : previousDayDepartmentConsumption.ConsumptionValue));

            var finalDepartmentStatus = ConsumptionStatus.Normal;
            if (log.Department.CurrentConsumptionLimit.HasValue)
            {
                foreach (var strategy in _strategies)
                {
                    var status = strategy.Evaluate(departmentConsumptionForToday, log.Department.CurrentConsumptionLimit ?? 0);

                    if (status > finalDepartmentStatus)
                    {
                        finalDepartmentStatus = status;
                    }
                }

            }

            
            var finalFactoryStatus = ConsumptionStatus.Normal;
            if (factory.CurrentConsumptionLimit.HasValue)
            {
                foreach (var strategy in _strategies)
                {
                    var status = strategy.Evaluate(factoryConsumptionForToday, factory.CurrentConsumptionLimit ?? 0);

                    if (status > finalFactoryStatus)
                    {
                        finalFactoryStatus = status;
                    }
                }
            }

            log.Status=finalDepartmentStatus;

            var result = await _unitOfWork.SaveChangesAsync();

            if (result <= 0)
            {
                return Result<ConsumptionLogDto>.Failure("Error Updating The log in the data base");
            }

            if (finalDepartmentStatus != ConsumptionStatus.Normal || finalFactoryStatus != ConsumptionStatus.Normal)
            {
                await _mediator.Publish(new HighConsumptionDetectedEvent(departmentConsumptionForToday, finalFactoryStatus, factoryId, finalDepartmentStatus, log.Department.Name, log.Department.Id, log.Id));
            }

            var logDto = _mapper.Map<ConsumptionLogDto>(log);

            return Result<ConsumptionLogDto>.Success(logDto);
        }

        public async Task<Result<bool>> DeleteConsumptionLogAsync(int logId)
        {
            var log = await _unitOfWork.ConsumptionLogs.GetByIdAsync(logId);

            if (log == null)
            {
                return Result<bool>.Failure("Log not found", 404);
            }

            _unitOfWork.ConsumptionLogs.Delete(log);
            var result = await _unitOfWork.SaveChangesAsync();

            return result > 0
                ? Result<bool>.Success(true)
                : Result<bool>.Failure("Error deleting the log");
        }
    }
}
