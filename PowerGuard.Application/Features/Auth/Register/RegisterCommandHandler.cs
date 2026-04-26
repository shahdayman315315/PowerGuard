using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Identity;
using PowerGuard.Application.Dtos;
using PowerGuard.Application.Helpers;
using PowerGuard.Application.Interfaces;
using PowerGuard.Domain.Interfaces;
using PowerGuard.Domain.Models;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PowerGuard.Application.Features.Auth.Register
{
    public class RegisterCommandHandler : IRequestHandler<RegisterCommand, Result<AuthResultDto>>
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ITokenService _tokenService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public RegisterCommandHandler(UserManager<ApplicationUser> userManager, ITokenService tokenService,
            IUnitOfWork unitOfWork,IMapper mapper)
        {
            _mapper = mapper;
            _userManager = userManager;
            _tokenService = tokenService;
            _unitOfWork = unitOfWork;
        }

        public async Task<Result<AuthResultDto>> Handle(RegisterCommand request, CancellationToken cancellationToken)
        {
            var existEmail = await _userManager.FindByEmailAsync(request.Email);

            if (existEmail is not null)
            {
                return Result<AuthResultDto>.Failure("Email already exists", 400);
            }

            var user = _mapper.Map<ApplicationUser>(request);

            var result = await _userManager.CreateAsync(user,request.Password);

            if (!result.Succeeded)
            {
                string errors = string.Join(", ", result.Errors.Select(e => e.Description));
                return  Result<AuthResultDto>.Failure(errors, 400);
               
            }

            await _userManager.AddToRoleAsync(user, "FactoryManager");

            var token = await _tokenService.GenerateToken(user);
            var refreshToken = _tokenService.GenerateRefreshToken();

            var refreshTokenEntity = new RefreshToken
            {
                Token = refreshToken,
                UserId = user.Id,
                ExpiresOn = DateTime.UtcNow.AddDays(7),
                CreatedOn = DateTime.UtcNow
            };

            await _unitOfWork.RefreshTokens.AddAsync(refreshTokenEntity);
            await _unitOfWork.SaveChangesAsync();

            var authDto = new AuthResultDto
            {
                UserName = user.UserName!,
                Token = new JwtSecurityTokenHandler().WriteToken(token),
                ExpirationDate = token.ValidTo,
                RefreshTokenExpiration = refreshTokenEntity.ExpiresOn,
                RefreshToken = refreshToken,
                Role = "FactoryManager",
                FactoryId = user.FactoryId ?? 0,
                DepartmentId = user.DepartmentId ?? 0

            };

            return Result<AuthResultDto>.Success(authDto, "User registered successfully");
            
        }
    }
}
