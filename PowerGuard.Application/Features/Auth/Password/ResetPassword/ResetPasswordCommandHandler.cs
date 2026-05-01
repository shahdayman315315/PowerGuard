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

namespace PowerGuard.Application.Features.Auth.Password.ResetPassword
{
    public class ResetPasswordCommandHandler :
        IRequestHandler<ResetPasswordCommand, Result<bool>>
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IUnitOfWork _unitOfWork;

        public ResetPasswordCommandHandler(UserManager<ApplicationUser> userManager, IUnitOfWork unitOfWork)
        {
            _userManager = userManager;
            _unitOfWork = unitOfWork;
        }

        public async Task<Result<bool>> Handle(ResetPasswordCommand request, CancellationToken cancellationToken)
        {
            var otp = await _unitOfWork.UserOTPs.Query.Include(o => o.User).FirstOrDefaultAsync(o => o.User.Email == request.Email && o.ResetToken == request.resetToken);

            if (otp is null || otp.ResetTokenExpiration < DateTime.UtcNow || otp.IsUsed)
            {
                return Result<bool>.Failure("Invalid or expired reset token");
            }

            var user = otp.User;

            var removeResult = await _userManager.RemovePasswordAsync(user);

            if (!removeResult.Succeeded)
            {
                return Result<bool>.Failure("Error clearing old password.");
            }

            var addResult = await _userManager.AddPasswordAsync(user, request.NewPassword);

            if (!addResult.Succeeded)
            {
                return Result<bool>.Failure("Error setting new password: " + string.Join(", ", addResult.Errors.Select(e => e.Description)));
            }

            otp.IsUsed = true;
            otp.ResetToken = null;

            _unitOfWork.UserOTPs.Update(otp);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result<bool>.Success(true, "Password reset successfully");
        }
    }
}
