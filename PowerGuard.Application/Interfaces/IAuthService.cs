using PowerGuard.Application.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PowerGuard.Application.Interfaces
{
    public interface IAuthService
    {
        Task<AuthResultDto> RegisterAsync(RegisterDto registerDto);
        Task<AuthResultDto> LoginAsync(LoginDto loginDto);
        Task<AuthResultDto> RefreshTokenAsync(RefreshTokenDto dto);
        Task<bool> RevokeRefreshTokenAsync(string refreshToken);
        Task<AuthResultDto> RequestPasswordResetAsync(ForgetPasswordDto dto);
        Task<AuthResultDto> VerifyOtpAsync(VerifyOtpDto verifyOtpDto);
        Task<AuthResultDto> ResetPasswordAsync(ResetPasswordDto resetDto);
    }
}
