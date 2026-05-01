using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
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

namespace PowerGuard.Application.Features.Auth.Password.ForgetPassword
{
    public class ForgetPasswordCommandHandler : IRequestHandler<ForgetPasswordCommand, Result<bool>>
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IEmailService _emailService;
        public ForgetPasswordCommandHandler(UserManager<ApplicationUser> userManager, IUnitOfWork unitOfWork, IEmailService emailService)
        {
            _userManager = userManager;
            _unitOfWork = unitOfWork;
            _emailService = emailService;
        }

        public async Task<Result<bool>> Handle(ForgetPasswordCommand request, CancellationToken cancellationToken)
        {
            var existUser = await _userManager.FindByEmailAsync(request.Email);

            if (existUser is null)
            {
                return Result<bool>.Failure("User with the provided email does not exist");
            }

            var oneMinuteAgo = DateTime.UtcNow.AddMinutes(-1);

            var requestCount = await _unitOfWork.UserOTPs.Query
                .CountAsync(x => x.UserId == existUser.Id && x.CreatedAt >= oneMinuteAgo);

            if (requestCount >= 3)
            {
                return Result<bool>.Failure("Too many requests. Please wait a minute before trying again.");
            }

            var otp = new Random().Next(100000, 999999).ToString();
            var otpEntity = new UserOTP
            {
                Code = otp,
                UserId = existUser.Id,
                ExpirationDate = DateTime.UtcNow.AddMinutes(5),
                CreatedAt = DateTime.UtcNow
            };

            await _unitOfWork.UserOTPs.AddAsync(otpEntity, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // Here you would typically send the OTP to the user's email address using an email service.
            await _emailService.SendEmailAsync(request.Email, "Password Reset OTP", $"Your OTP for password reset is: {otp}");

             return Result<bool>.Success(true,"OTP sent to email successfully!");
        }
    }
}
