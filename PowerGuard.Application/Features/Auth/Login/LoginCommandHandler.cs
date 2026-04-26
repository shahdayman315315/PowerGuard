using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using PowerGuard.Application.Dtos;
using PowerGuard.Application.Helpers;
using PowerGuard.Application.Interfaces;
using PowerGuard.Domain.Enums;
using PowerGuard.Domain.Interfaces;
using PowerGuard.Domain.Models;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PowerGuard.Application.Features.Auth.Login
{
    public class LoginCommandHandler : IRequestHandler<LoginCommand, Result<AuthResultDto>>
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ITokenService _tokenService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public LoginCommandHandler(UserManager<ApplicationUser> userManager, ITokenService tokenService,
            IUnitOfWork unitOfWork, IMapper mapper)
        {
            _mapper = mapper;
            _userManager = userManager;
            _tokenService = tokenService;
            _unitOfWork = unitOfWork;
        }
        public async Task<Result<AuthResultDto>> Handle(LoginCommand request, CancellationToken cancellationToken)
        {
            var existUser = await _userManager.FindByEmailAsync(request.Email);

            if (existUser is null || !await _userManager.CheckPasswordAsync(existUser, request.Password))
            {
                return Result<AuthResultDto>.Failure("Invalid email or password");
            }

            var userRole = (await _userManager.GetRolesAsync(existUser)).FirstOrDefault();
            if (userRole == "FactoryManager")
            {
                var managedFactory = await _unitOfWork.Factories.Query.FirstOrDefaultAsync(f => f.ManagerId == existUser.Id);

                if (managedFactory is null)
                {
                    return Result<AuthResultDto>.Failure("No factory associated with this user");
                }

                if (managedFactory.Status == FactoryStatus.Pending)
                {
                    return Result<AuthResultDto>.Failure("Your Request for factory creation is under review");
                }

                else if (managedFactory.Status == FactoryStatus.Rejected || managedFactory.Status == FactoryStatus.Suspended || managedFactory.Status == FactoryStatus.Deactivated)
                {
                    return Result<AuthResultDto>.Failure("You can't use our service as your factory is currently inactive ");
                }

            }

            else if (userRole == "DepartmentManager")
            {
                var department = await _unitOfWork.Departments.Query.Include(d => d.Factory).FirstOrDefaultAsync(d => d.ManagerId == existUser.Id);

                var Factory = department.Factory;

                if (Factory.Status == FactoryStatus.Rejected || Factory.Status == FactoryStatus.Suspended || Factory.Status == FactoryStatus.Deactivated)
                {
                    return Result<AuthResultDto>.Failure("You can't use our service as your factory is currently inactive ");
                }


            }

            var token = await _tokenService.GenerateToken(existUser);
            var refreshToken = _tokenService.GenerateRefreshToken();
            var refreshTokenEntity = new RefreshToken
            {
                Token = refreshToken,
                UserId = existUser.Id,
                ExpiresOn = DateTime.UtcNow.AddDays(7),
                CreatedOn = DateTime.UtcNow
            };

            await _unitOfWork.RefreshTokens.AddAsync(refreshTokenEntity);
            await _unitOfWork.SaveChangesAsync();

            var authDto = new AuthResultDto
            {
                UserName = existUser.UserName!,
                Token = new JwtSecurityTokenHandler().WriteToken(token),
                ExpirationDate = token.ValidTo,
                RefreshTokenExpiration = refreshTokenEntity.ExpiresOn,
                RefreshToken = refreshToken,
                Role = (await _userManager.GetRolesAsync(existUser)).FirstOrDefault() ?? string.Empty,
                FactoryId = existUser.FactoryId ?? 0,
                DepartmentId = existUser.DepartmentId ?? 0
            };

            return Result<AuthResultDto>.Success(authDto, "Logged in successfully");

            
        }
    }
}
