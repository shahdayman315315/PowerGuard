using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using PowerGuard.Application.Dtos;
using PowerGuard.Application.Helpers;
using PowerGuard.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PowerGuard.Application.Features.Factory.UpdateFactory
{
    public class UpdateFactoryCommandHandler : IRequestHandler<UpdateFactoryCommand, Result<FactoryDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ICacheService _cache;

        public UpdateFactoryCommandHandler(IUnitOfWork unitOfWork, IMapper mapper, ICacheService cache)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _cache = cache;
        }
        public async Task<Result<FactoryDto>> Handle(UpdateFactoryCommand request, CancellationToken cancellationToken)
        {
            var factory = await _unitOfWork.Factories.Query.Include(f => f.Departments).FirstOrDefaultAsync(f => f.Id == request.FactoryId);

            if (factory is null)
            {
                return Result<FactoryDto>.Failure("Factory not found.", 404);
            }

            _mapper.Map(request, factory);
            _unitOfWork.Factories.Update(factory);
            var result = await _unitOfWork.SaveChangesAsync(cancellationToken);

            if (result > 0)
            {
                await _cache.RemoveAsync("ActiveFactoriesList");
                await _cache.RemoveAsync("FactoriesList");

                var factoryDto = _mapper.Map<FactoryDto>(factory);
                return Result<FactoryDto>.Success(factoryDto);
            }

            return Result<FactoryDto>.Failure("Failed to update the factory in the database.");
        }
    }
    
}
