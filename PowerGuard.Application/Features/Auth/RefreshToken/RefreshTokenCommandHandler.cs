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
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace PowerGuard.Application.Features.Auth.RefreshToken
{
    public class RefreshTokenCommandHandler : IRequestHandler<RefreshTokenCommand, Result<AuthResultDto>>
    {
        private readonly ITokenService _tokenService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IEmailService _emailService;
        public RefreshTokenCommandHandler(ITokenService tokenService, UserManager<ApplicationUser> userManager,
            IMapper mapper, IUnitOfWork unitOfWork, IEmailService emailService)
        {
            _emailService = emailService;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _userManager = userManager;
            _tokenService = tokenService;
        }
        public async Task<Result<AuthResultDto>> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
        {
            var principal = _tokenService.GetPrincipalFromExpiredToken(request.AccessToken);

            if (principal is null)
            {
                return Result<AuthResultDto>.Failure("Invalid access token");
            }

            var userId = principal.FindFirstValue(ClaimTypes.NameIdentifier);

            var user = await _userManager.FindByIdAsync(userId);

            if (user is null)
            {
                return Result<AuthResultDto>.Failure("User not found");
            }

            var storedRefreshToken = _unitOfWork.RefreshTokens.Query.FirstOrDefault(t => t.Token == request.RefreshToken && t.UserId == userId);

            if (storedRefreshToken is null || !storedRefreshToken.IsActive)
            {
                return Result<AuthResultDto>.Failure("Invalid or expired refresh token");
            }

            storedRefreshToken.RevokedOn = DateTime.UtcNow;

            var jwtToken = await _tokenService.GenerateToken(user);

            var refreshToken = _tokenService.GenerateRefreshToken();
            var refreshTokenEntity = new Domain.Models.RefreshToken
            {
                Token = refreshToken,
                UserId = user.Id,
                ExpiresOn = DateTime.UtcNow.AddDays(7),
                CreatedOn = DateTime.UtcNow,
            };

            user.RefreshTokens.Add(refreshTokenEntity);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var authDto=new  AuthResultDto
            {
                Token = new JwtSecurityTokenHandler().WriteToken(jwtToken),
                Role = (await _userManager.GetRolesAsync(user)).FirstOrDefault() ?? string.Empty,
                UserName = user.UserName!,
                RefreshToken = refreshToken,
                RefreshTokenExpiration = refreshTokenEntity.ExpiresOn,
                ExpirationDate = jwtToken.ValidTo,
                FactoryId = user.FactoryId ?? 0,
                DepartmentId = user.DepartmentId ?? 0

            };

            return Result<AuthResultDto>.Success(authDto, "Token refreshed successfully");
        }
    }
}
