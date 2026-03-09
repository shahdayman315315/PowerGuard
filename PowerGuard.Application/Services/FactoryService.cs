using AutoMapper;
using PowerGuard.Application.Dtos;
using PowerGuard.Application.Helpers;
using PowerGuard.Application.Interfaces;
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
        private readonly IMapper _mapper;
        public FactoryService(IUnitOfWork unitOfWork , IMapper mapper)
        {
            _mapper = mapper;
            _unitOfWork = unitOfWork;
        }
        public async Task<Result<CreateFactoryDto>> CreateFactory(CreateFactoryDto dto,string userId)
        {
            if (string.IsNullOrEmpty(userId))
            {
                return Result<CreateFactoryDto>.Failure("User ID is missing.");
            }

            var factory = _mapper.Map<Factory>(dto);
            factory.ManagerId= userId;

            await _unitOfWork.Factories.AddAsync(factory);
            var result=await _unitOfWork.SaveChangesAsync();

            if (result > 0)
            {
                return Result<CreateFactoryDto>.Success(dto);
            }

            return Result<CreateFactoryDto>.Failure("Failed to save the factory to the database.");
        }

        public Task<Result<bool>> DeleteFactory(int id)
        {
            throw new NotImplementedException();
        }

        public async Task<Result<List<FactoryDto>>> GetAllFactories()
        {
            var factories = await _unitOfWork.Factories.GetAll().Result;
            throw new NotImplementedException();
        }

        public async Task<Result<FactoryDto>> GetFactoryById(int id)
        {
            var factory = await _unitOfWork.Factories.GetByIdAsync(id);

            if(factory is null)
            {
                return Result<FactoryDto>.Failure("Factory not found.");
            }

            var factoryDto = _mapper.Map<FactoryDto>(factory);

            return Result<FactoryDto>.Success(factoryDto);
        }

        public Task<Result<FactoryDto>> UpdateFactory(int id, UpdateFactoryDto dto)
        {
            throw new NotImplementedException();
        }
    }
}
