using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using PowerGuard.Application.Dtos;
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

namespace PowerGuard.Application.Services
{
    public class FactoryService : IFactoryService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;
        private readonly IEnumerable<IConsumptionEvaluationStrategy> _strategies;
        private readonly IMemoryCache _memoryCache;

        public FactoryService(IUnitOfWork unitOfWork , IMapper mapper, UserManager<ApplicationUser> userManager
            ,IMediator mediator, IEnumerable<IConsumptionEvaluationStrategy> strategies, IMemoryCache memoryCache)
        {
            _mapper = mapper;
            _unitOfWork = unitOfWork;
            _userManager = userManager;
            _mediator = mediator;
            _strategies = strategies;
            _memoryCache = memoryCache;
        }
        public async Task<Result<CreateFactoryDto>> CreateFactory(CreateFactoryDto dto,string userId)
        {
            
        }

        public async Task<Result<bool>> DeleteFactory(int id)
        {
            

        }

        public async Task<Result<List<FactoryDto>>> GetActiveFactories()
        {
           
        }

        public async Task<Result<List<FactoryDto>>> GetAllFactories()
        {
            
        }

        public async Task<Result<FactoryDto>> GetFactoryById(int id)
        {
            
        }

        public async Task<Result<List<FactoryDetailsDto>>> GetPendingFactories()
        {
            
        }

        public async Task<Result<bool>> UpdateConsumptionLimit(int factoryId, UpdateConsumptionLimitDto dto,string userId)
        {
            
        }



        public async Task<Result<FactoryDto>> UpdateFactory(int id, UpdateFactoryDto dto)
        {
        }
    }
}
