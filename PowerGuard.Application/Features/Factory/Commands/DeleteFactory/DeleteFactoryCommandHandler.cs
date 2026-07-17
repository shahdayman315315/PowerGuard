using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using PowerGuard.Application.Helpers;
using PowerGuard.Domain.Enums;
using PowerGuard.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PowerGuard.Application.Features.Factory.DeleteFactory
{
    public class DeleteFactoryCommandHandler : IRequestHandler<DeleteFactoryCommand, Result<bool>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMemoryCache _memoryCache;

        public DeleteFactoryCommandHandler(IUnitOfWork unitOfWork, IMemoryCache memoryCache)
        {
            _unitOfWork = unitOfWork;
            _memoryCache = memoryCache;
        }
        public async Task<Result<bool>> Handle(DeleteFactoryCommand request, CancellationToken cancellationToken)
        {
            var factory = await _unitOfWork.Factories.Query.Include(f => f.Manager)
                .Include(f => f.Departments)
                .ThenInclude(d => d.Manager)
                .FirstOrDefaultAsync(f => f.Id == request.FactoryId);

            if (factory is null)
            {
                return Result<bool>.Failure("Factory not found.", 404);
            }

            factory.Status = FactoryStatus.Deactivated;
            _unitOfWork.Factories.Update(factory);
            //تعطيل مدير المصنع
            if (factory.Manager != null)
            {
                factory.Manager.LockoutEnd = DateTimeOffset.MaxValue; // قفل الحساب
            }

            // تعطيل مديري الأقسام
            foreach (var dept in factory.Departments)
            {
                if (dept.Manager != null)
                {
                    dept.Manager.LockoutEnd = DateTimeOffset.MaxValue;
                }
            }

            var result = await _unitOfWork.SaveChangesAsync(cancellationToken);

            if (result > 0)
            {
                _memoryCache.Remove("ActiveFactoriesList");
                return Result<bool>.Success(true);
            }

            return Result<bool>.Failure("Failed to deactivate the factory in the database.");
        }
    }
}
