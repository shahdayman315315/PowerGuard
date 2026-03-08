using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PowerGuard.Application.Dtos;
using PowerGuard.Application.Interfaces;

namespace PowerGuard.WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterDto registerDto)
        {
            var result=await _authService.RegisterAsync(registerDto);

            if (!result.IsSuccess)
            {
                return BadRequest(result.Message);
            }

            if (!string.IsNullOrEmpty(result.RefreshToken))
            {
                SetRefreshTokenInCookie(result.RefreshToken, result.RefreshTokenExpiration);
            }

            return Ok(result);
        }


        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDto dto)
        {
            var result = await _authService.LoginAsync(dto);

            if (!result.IsSuccess)
            {
                return Unauthorized(result.Message);
            }

            if (!string.IsNullOrEmpty(result.RefreshToken))
            {
                SetRefreshTokenInCookie(result.RefreshToken, result.RefreshTokenExpiration);
            }

            return Ok(result);
        }


        [HttpPost("refresh-token")]
        public async Task<IActionResult> RefreshToken(RefreshTokenDto? dto)
        {
            if(dto is null)
            {
                dto=new RefreshTokenDto
                {
                    AccessToken = Request.Headers["Authorization"].ToString().Replace("Bearer ", ""),
                    RefreshToken = Request.Cookies["refreshToken"]
                };
            }
            
            if (string.IsNullOrEmpty(dto.RefreshToken) || string.IsNullOrEmpty(dto.AccessToken))
            {
                return BadRequest("Tokens are required");
            }

            var result = await _authService.RefreshTokenAsync(dto);

            if (!result.IsSuccess)
            {
                return Unauthorized(result.Message);
            }

            if (!string.IsNullOrEmpty(result.RefreshToken))
            {
                SetRefreshTokenInCookie(result.RefreshToken, result.RefreshTokenExpiration);
            }

            return Ok(result);
        }


        [HttpPost("revoke-token")]
        public async Task<IActionResult> RevokeToken([FromBody] string? refreshToken)
        {
            var token = refreshToken ?? Request.Cookies["refreshToken"];

            if (string.IsNullOrEmpty(token))
            {
                return BadRequest("Token is required");
            }

            var result = await _authService.RevokeRefreshTokenAsync(token);

            if (!result)
            {
                return BadRequest("Token revocation failed");
            }


            return Ok("Token revoked successfully");
        }



        [HttpPost("logout")]
        public async Task<IActionResult> Logout([FromBody] string? refreshToken)
        {
            var token = refreshToken ?? Request.Cookies["refreshToken"];

            if (string.IsNullOrEmpty(token))
            {
                return BadRequest("Token is required");
            }

            var result = await _authService.RevokeRefreshTokenAsync(token);

            if (!result)
            {
                return BadRequest("Logout failed");
            }

            Response.Cookies.Delete("refreshToken");

            return Ok("Logged out successfully");
        }

        [HttpPost("forget-password")]
        public async Task<IActionResult> ForgetPassword(ForgetPasswordDto dto)
        {
            var result = await _authService.RequestPasswordResetAsync(dto);

            if (!result.IsSuccess)
            {
                return BadRequest(result.Message);
            }

            return Ok(result.Message);
        }

        [HttpPost("verify-otp")]
        public async Task<IActionResult> VerifyOtp(VerifyOtpDto dto)
        {
            var result = await _authService.VerifyOtpAsync(dto);

            if (!result.IsSuccess)
            {
                return BadRequest(result.Message);
            }
            
            return Ok(result);
        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword(ResetPasswordDto dto)
        {
            var result = await _authService.ResetPasswordAsync(dto);

            if (!result.IsSuccess)
            {
                return BadRequest(result.Message);
            }

            return Ok(result.Message);
        }

        private void SetRefreshTokenInCookie(string refreshToken, DateTime ExpiresOn)
        {
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Expires = ExpiresOn,
                Secure = true,
                SameSite = SameSiteMode.Strict
            };

            Response.Cookies.Append("refreshToken", refreshToken, cookieOptions);
        }
    }
}
