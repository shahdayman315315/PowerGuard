using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Caching.Memory;
using PowerGuard.Application.Dtos;
using PowerGuard.Application.Helpers;
using PowerGuard.Application.Interfaces;
using PowerGuard.Domain.Interfaces;
using PowerGuard.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace PowerGuard.Application.Features.Factory.CreateFactory
{
    public class CreateFactoryCommandHandler : IRequestHandler<CreateFactoryCommand, Result<CreateFactoryDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;
        private readonly IEnumerable<IConsumptionEvaluationStrategy> _strategies;
        private readonly IMemoryCache _memoryCache;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CreateFactoryCommandHandler(IUnitOfWork unitOfWork, IMapper mapper, UserManager<ApplicationUser> userManager
            , IMediator mediator, IEnumerable<IConsumptionEvaluationStrategy> strategies, IMemoryCache memoryCache, IHttpContextAccessor httpContextAccessor)
        {
            _mapper = mapper;
            _unitOfWork = unitOfWork;
            _userManager = userManager;
            _mediator = mediator;
            _strategies = strategies;
            _memoryCache = memoryCache;
            _httpContextAccessor = httpContextAccessor;
        }
        public async Task<Result<CreateFactoryDto>> Handle(CreateFactoryCommand request, CancellationToken cancellationToken)
        {
            var userId= _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            var factory = _mapper.Map<Domain.Models.Factory>(request);
            factory.ManagerId = userId!;

            await _unitOfWork.Factories.AddAsync(factory,cancellationToken);
            var result = await _unitOfWork.SaveChangesAsync(cancellationToken);

            if (result > 0)
            {
                var manager = await _userManager.FindByIdAsync(userId!);
                manager!.FactoryId = factory.Id;

                var updateResult = await _userManager.UpdateAsync(manager);

                if (!updateResult.Succeeded)
                {
                    return Result<CreateFactoryDto>.Failure("Error happened while setting the manager");
                }

                _memoryCache.Remove("ActiveFactoriesList");
                _memoryCache.Remove("FactoriesList");

                var dto = _mapper.Map<CreateFactoryDto>(factory);

                return Result<CreateFactoryDto>.Success(dto);
            }

            return Result<CreateFactoryDto>.Failure("Failed to save the factory to the database.");
        }
    }
}
