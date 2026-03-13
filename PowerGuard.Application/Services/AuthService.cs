using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Org.BouncyCastle.Pqc.Crypto.Falcon;
using PowerGuard.Application.Dtos;
using PowerGuard.Application.Interfaces;
using PowerGuard.Domain.Enums;
using PowerGuard.Domain.Interfaces;
using PowerGuard.Domain.Models;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace PowerGuard.Application.Services
{
    public class AuthService : IAuthService
    {
        private readonly ITokenService _tokenService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IEmailService _emailService;   
        public AuthService(ITokenService tokenService, UserManager<ApplicationUser> userManager,
            IMapper mapper, IUnitOfWork unitOfWork, IEmailService emailService)
        {
            _tokenService = tokenService;
            _userManager = userManager;
            _mapper = mapper;
            _unitOfWork = unitOfWork;
            _emailService = emailService;
        }
        public async Task<AuthResultDto> LoginAsync(LoginDto loginDto)
        {
            var existUser=await _userManager.FindByEmailAsync(loginDto.Email);

            if(existUser is null || !await _userManager.CheckPasswordAsync(existUser, loginDto.Password))
            {
                return new AuthResultDto { Message = "Invalid email or password" };
            }

            var userRole=(await _userManager.GetRolesAsync(existUser)).FirstOrDefault();
            if (userRole == "FactoryManager")
            {
                var managedFactory = await _unitOfWork.Factories.Query.FirstOrDefaultAsync(f => f.ManagerId == existUser.Id);

                if(managedFactory is null)
                {
                    return new AuthResultDto { Message = "No factory associated with this user" };
                }

                if (managedFactory.Status == FactoryStatus.Pending)
                {
                    return new AuthResultDto { Message = "Your Request for factory creation is under review" };
                }

                else if (managedFactory.Status == FactoryStatus.Rejected || managedFactory.Status == FactoryStatus.Suspended || managedFactory.Status == FactoryStatus.Deactivated)
                {
                    return new AuthResultDto { Message = "You can't use our service as your factory is inactive currently" };
                }

            }

            else if (userRole == "DepartmentManager")
            {
                var department = await _unitOfWork.Departments.Query.Include(d => d.Factory).FirstOrDefaultAsync(d => d.ManagerId == existUser.Id);

                var Factory = department.Factory;

                if (Factory.Status == FactoryStatus.Rejected || Factory.Status == FactoryStatus.Suspended || Factory.Status == FactoryStatus.Deactivated)
                {
                    return new AuthResultDto { Message = "You can't use our service as your factory is inactive currently" };
                }

            
            }

            var token=await _tokenService.GenerateToken(existUser);
            var refreshToken=_tokenService.GenerateRefreshToken();
            var refreshTokenEntity = new RefreshToken
            {
                Token = refreshToken,
                UserId = existUser.Id,
                ExpiresOn = DateTime.UtcNow.AddDays(7),
                CreatedOn = DateTime.UtcNow
            };

            await _unitOfWork.RefreshTokens.AddAsync(refreshTokenEntity);
            await _unitOfWork.SaveChangesAsync();

            return new AuthResultDto
            {
                UserName = existUser.UserName,
                Token = new JwtSecurityTokenHandler().WriteToken(token),
                ExpirationDate = token.ValidTo,
                Message = "Login successful",
                RefreshTokenExpiration = refreshTokenEntity.ExpiresOn,
                RefreshToken = refreshToken,
                Role= (await _userManager.GetRolesAsync(existUser)).FirstOrDefault() ?? string.Empty,
                IsSuccess = true,
                FactoryId = existUser.FactoryId??0
            };
        }

        public async Task<AuthResultDto> RefreshTokenAsync(RefreshTokenDto dto)
        {
            var principal = _tokenService.GetPrincipalFromExpiredToken(dto.AccessToken);

            if (principal is null)
            {
                return new AuthResultDto { Message = "Invalid access token" };
            }

            var userId = principal.FindFirstValue(ClaimTypes.NameIdentifier);

            var user = await _userManager.FindByIdAsync(userId);

            if (user is null)
            {
                return new AuthResultDto { Message = "User not found" };
            }

            var storedRefreshToken = _unitOfWork.RefreshTokens.Query.FirstOrDefault(t => t.Token == dto.RefreshToken && t.UserId==userId);

            if (storedRefreshToken is null || !storedRefreshToken.IsActive)
            {
                return new AuthResultDto { Message = "Invalid or expired refresh token" };
            }

            storedRefreshToken.RevokedOn = DateTime.UtcNow;

            var jwtToken = await _tokenService.GenerateToken(user);

            var refreshToken = _tokenService.GenerateRefreshToken();
            var refreshTokenEntity = new RefreshToken
            {
                Token = refreshToken,
                UserId = user.Id,
                ExpiresOn = DateTime.UtcNow.AddDays(7),
                CreatedOn = DateTime.UtcNow,
            };

            user.RefreshTokens.Add(refreshTokenEntity);
            await _unitOfWork.SaveChangesAsync();

            return new AuthResultDto
            {
                Token = new JwtSecurityTokenHandler().WriteToken(jwtToken),
                IsSuccess = true,
                Message = "Token refreshed successfully",
                UserName = user.UserName,
                RefreshToken = refreshToken,
                RefreshTokenExpiration = refreshTokenEntity.ExpiresOn,
                ExpirationDate = jwtToken.ValidTo,
                FactoryId = user.FactoryId ?? 0
            };

        }

        public async Task<AuthResultDto> RegisterAsync(RegisterDto registerDto)
        {
            var existEmail=await _userManager.FindByEmailAsync(registerDto.Email);  

            if(existEmail is not null)
            {
                return new AuthResultDto { Message = "Email already exists" };
            }

            var user=_mapper.Map<ApplicationUser>(registerDto);

            var result=await _userManager.CreateAsync(user,registerDto.Password);

            if (!result.Succeeded)
            {
                string errors = string.Join(", ", result.Errors.Select(e => e.Description));
                return new AuthResultDto { Message = errors };
            }

            await _userManager.AddToRoleAsync(user, "FactoryManager");

            var token=await _tokenService.GenerateToken(user);
            var refreshToken=_tokenService.GenerateRefreshToken();

            var refreshTokenEntity = new RefreshToken
            {
                Token = refreshToken,
                UserId = user.Id,
                ExpiresOn = DateTime.UtcNow.AddDays(7),
                CreatedOn = DateTime.UtcNow
            };

            await _unitOfWork.RefreshTokens.AddAsync(refreshTokenEntity);
            await _unitOfWork.SaveChangesAsync();

            return new AuthResultDto
            {
                UserName = user.UserName,
                Token = new JwtSecurityTokenHandler().WriteToken(token),
                ExpirationDate = token.ValidTo,
                Message = "User registered successfully",
                RefreshTokenExpiration = refreshTokenEntity.ExpiresOn,
                RefreshToken = refreshToken,
                Role= "FactoryManager",
                IsSuccess = true,
                FactoryId = user.FactoryId ?? 0
            };
        }

        public async Task<AuthResultDto> RequestPasswordResetAsync(ForgetPasswordDto dto)
        {
            var existUser = await _userManager.FindByEmailAsync(dto.Email);

            if(existUser is null)
            {
                return new AuthResultDto { Message = "User with the provided email does not exist" };
            }

            var oneMinuteAgo = DateTime.UtcNow.AddMinutes(-1);

            var requestCount = await _unitOfWork.UserOTPs.Query
                .CountAsync(x => x.UserId == existUser.Id && x.CreatedAt >= oneMinuteAgo);

            if (requestCount >= 3)
            {
                return new AuthResultDto
                {
                    IsSuccess = false,
                    Message = "Too many requests. Please wait a minute before trying again."
                };
            }

            var otp = new Random().Next(100000, 999999).ToString();
            var otpEntity = new UserOTP
            {
                Code = otp,
                UserId = existUser.Id,
                ExpirationDate = DateTime.UtcNow.AddMinutes(5),
                CreatedAt = DateTime.UtcNow
            };

                await _unitOfWork.UserOTPs.AddAsync(otpEntity);
                await _unitOfWork.SaveChangesAsync();

            // Here you would typically send the OTP to the user's email address using an email service.
            await _emailService.SendEmailAsync(dto.Email, "Password Reset OTP", $"Your OTP for password reset is: {otp}");
         
            return new AuthResultDto { IsSuccess = true ,Message = "OTP sent to email successfully" };
        }

        public async Task<AuthResultDto> ResetPasswordAsync(ResetPasswordDto resetDto)
        {
            var otp = await _unitOfWork.UserOTPs.Query.Include(o => o.User).FirstOrDefaultAsync(o => o.User.Email == resetDto.Email && o.ResetToken == resetDto.ResetToken);
                
            if (otp is null || otp.ResetTokenExpiration < DateTime.UtcNow ||otp.IsUsed)
            {
                return new AuthResultDto { Message = "Invalid or expired reset token" };
            }

            var user=otp.User;

            var removeResult=await _userManager.RemovePasswordAsync(user);

            if(!removeResult.Succeeded)
            {
                return new AuthResultDto { Message = "Error clearing old password." };
            }

            var addResult = await _userManager.AddPasswordAsync(user, resetDto.NewPassword);

            if(!addResult.Succeeded)
            {
                return new AuthResultDto { Message = string.Join(", ", addResult.Errors.Select(e => e.Description)) };
            }

            otp.IsUsed = true;
            otp.ResetToken = null;

            _unitOfWork.UserOTPs.Update(otp);
            await _unitOfWork.SaveChangesAsync();

            return new AuthResultDto { IsSuccess = true, Message = "Password reset successfully!" };
        }

        public async Task<bool> RevokeRefreshTokenAsync(string refreshToken)
        {
            var existRefreshToken = await _unitOfWork.RefreshTokens.Query
             .FirstOrDefaultAsync(t => t.Token == refreshToken);

            if (existRefreshToken is null || !existRefreshToken.IsActive)
                return false;

            existRefreshToken.RevokedOn = DateTime.UtcNow;

            _unitOfWork.RefreshTokens.Update(existRefreshToken);
            await _unitOfWork.SaveChangesAsync();

            return true;
        }

        public async Task<AuthResultDto> VerifyOtpAsync(VerifyOtpDto verifyOtpDto)
        {
            var existUser = await _userManager.FindByEmailAsync(verifyOtpDto.Email);

            if (existUser is null)
            {
                return new AuthResultDto { Message = "User with the provided email does not exist" };
            }
            
            var otp=await _unitOfWork.UserOTPs.Query.Where(o => o.UserId == existUser.Id && !o.IsUsed).
                OrderByDescending(o=>o.CreatedAt).FirstOrDefaultAsync();
           
            if (otp is null || otp.Code!=verifyOtpDto.Otp ||otp.ExpirationDate<DateTime.UtcNow )
            {
                return new AuthResultDto { Message = "Invalid OTP" };
            }

            var resetToken = Guid.NewGuid().ToString();
            otp.ResetToken = resetToken;
            otp.ResetTokenExpiration = DateTime.UtcNow.AddMinutes(5);

            _unitOfWork.UserOTPs.Update(otp);
            await _unitOfWork.SaveChangesAsync();

            return new AuthResultDto { IsSuccess = true, Message = "OTP verified successfully",Token=otp.ResetToken };
        }
    }
}
