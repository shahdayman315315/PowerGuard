using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using PowerGuard.Application.Dtos;
using PowerGuard.Application.Helpers;
using PowerGuard.Domain.Interfaces;
using PowerGuard.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PowerGuard.Application.Features.Auth.Password.VerifyOtp
{
    public class VerifyOtpCommandHandler :
        IRequestHandler<VerifyOtpCommand, Result<AuthResultDto>>
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IUnitOfWork _unitOfWork;
        
        public VerifyOtpCommandHandler(UserManager<ApplicationUser> userManager, IUnitOfWork unitOfWork)
        {
            _userManager = userManager;
            _unitOfWork = unitOfWork;
        }
        public async Task<Result<AuthResultDto>> Handle(VerifyOtpCommand request, CancellationToken cancellationToken)
        {
            var existUser = await _userManager.FindByEmailAsync(request.Email);

            if (existUser is null)
            {
                return Result<AuthResultDto>.Failure("User with the provided email does not exist");
            }

            var otp = await _unitOfWork.UserOTPs.Query.Where(o => o.UserId == existUser.Id && !o.IsUsed).
                OrderByDescending(o => o.CreatedAt).FirstOrDefaultAsync();

            if (otp is null || otp.Code != request.Otp || otp.ExpirationDate < DateTime.UtcNow)
            {
                return Result<AuthResultDto>.Failure("Invalid OTP");
            }

            var resetToken = Guid.NewGuid().ToString();
            otp.ResetToken = resetToken;
            otp.ResetTokenExpiration = DateTime.UtcNow.AddMinutes(5);

            _unitOfWork.UserOTPs.Update(otp);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var authDto= new AuthResultDto { Token = otp.ResetToken };

            return Result<AuthResultDto>.Success(authDto,"Otp verified Successfully.");

        }
    }
}
