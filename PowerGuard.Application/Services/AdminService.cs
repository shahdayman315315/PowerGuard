using Microsoft.EntityFrameworkCore;
using PowerGuard.Application.Dtos;
using PowerGuard.Application.Helpers;
using PowerGuard.Application.Interfaces;
using PowerGuard.Domain.Enums;
using PowerGuard.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PowerGuard.Application.Services
{
    public class AdminService : IAdminService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IEmailService _emailService;
        public AdminService(IUnitOfWork unitOfWork, IEmailService emailService)
        {
            _unitOfWork = unitOfWork;
            _emailService = emailService;
        }
        public async Task<Result<AdminDashboardDto>> GetAdminDashboardStats()
        {
            var statsQuery = await _unitOfWork.Factories.Query
                .GroupBy(f => f.Status)
                .Select(group => new
                {
                    Status = group.Key,
                    Count = group.Count()
                })
                .ToListAsync();

            var dto = new AdminDashboardDto
            {
                TotalFactories = statsQuery.Sum(x => x.Count),
                PendingFactories = statsQuery.FirstOrDefault(x => x.Status == FactoryStatus.Pending)?.Count ?? 0,
                ActiveFactories = statsQuery.FirstOrDefault(x => x.Status == FactoryStatus.Approved)?.Count ?? 0
            };

            return Result<AdminDashboardDto>.Success(dto);
        }

        public async Task<Result<bool>> ReviewFactory(ReviewFactoryDto dto)
        {
            var factory= await _unitOfWork.Factories.Query.Include(f=>f.Manager).FirstOrDefaultAsync(f=>f.Id==dto.FactoryId);

            if(factory is null)
            {
                return Result<bool>.Failure("Factory not found.",404);
            }

            if(factory.Status != FactoryStatus.Pending)
            {
                return Result<bool>.Failure("Only pending factories can be reviewed.");
            }

            if(dto.IsApproved)
            {
                factory.Status = FactoryStatus.Approved;
                await _emailService.SendEmailAsync(factory.Manager.Email!, "Factory Approve", "Congrats! Ur factory has been approved by the admin.");
            }
            else
            {
                factory.Status = FactoryStatus.Rejected;
                factory.AdminRemarks = dto.AdminRemarks ?? "No remarks provided.";
                await _emailService.SendEmailAsync(factory.Manager.Email!, "Factory Rejection", $"Ur Factory has been rejected due to : {dto.AdminRemarks}");
            }

            var result= await _unitOfWork.SaveChangesAsync();

            if(result > 0)
            {
                return Result<bool>.Success(true);
            }

            return Result<bool>.Failure("Can't update factory status in the database.");
        }
    }
}
