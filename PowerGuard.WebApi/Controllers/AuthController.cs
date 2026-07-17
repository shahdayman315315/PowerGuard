using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PowerGuard.Application.Dtos;
using PowerGuard.Application.Features.Auth.Login;
using PowerGuard.Application.Features.Auth.Password.ForgetPassword;
using PowerGuard.Application.Features.Auth.Password.ResetPassword;
using PowerGuard.Application.Features.Auth.Password.VerifyOtp;
using PowerGuard.Application.Features.Auth.RefreshToken;
using PowerGuard.Application.Features.Auth.Register;
using PowerGuard.Application.Features.Auth.RevokeRefreshToken;
using PowerGuard.Application.Interfaces;

namespace PowerGuard.WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly ISender _sender;
        public AuthController(ISender sender)
        {
            _sender = sender;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterCommand command)
        {

            var result=await _sender.Send(command);

            if (!result.IsSuccess)
            {
                return BadRequest(result.Message);
            }

            if (!string.IsNullOrEmpty(result.Data!.RefreshToken))
            {
                SetRefreshTokenInCookie(result.Data!.RefreshToken, result.Data!.RefreshTokenExpiration);
            }

            return Ok(result.Data);
        }


        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginCommand command)
        {
            var result = await _sender.Send(command);

            if (!result.IsSuccess)
            {
                return Unauthorized(result.Message);
            }

            if (!string.IsNullOrEmpty(result.Data!.RefreshToken))
            {
                SetRefreshTokenInCookie(result.Data!.RefreshToken, result.Data!.RefreshTokenExpiration);
            }

            return Ok(result.Data);
        }


        [HttpPost("refresh-token")]
        public async Task<IActionResult> RefreshToken(RefreshTokenCommand command)
        {
            if(command is null)
            {
                command = new RefreshTokenCommand
                    (
                    Request.Headers["Authorization"].ToString().Replace("Bearer ", ""),
                    Request.Cookies["refreshToken"]
                    );
                
            }
            
            if (string.IsNullOrEmpty(command.RefreshToken) || string.IsNullOrEmpty(command.AccessToken))
            {
                return BadRequest("Tokens are required");
            }

            var result = await _sender.Send(command);

            if (!result.IsSuccess)
            {
                return Unauthorized(result.Message);
            }

            if (!string.IsNullOrEmpty(result.Data!.RefreshToken))
            {
                SetRefreshTokenInCookie(result.Data!.RefreshToken, result.Data!.RefreshTokenExpiration);
            }

            return Ok(result.Data);
        }


        [HttpPost("revoke-token")]
        public async Task<IActionResult> RevokeToken([FromBody] string? refreshToken)
        {
            var token = refreshToken ?? Request.Cookies["refreshToken"];

            if (string.IsNullOrEmpty(token))
            {
                return BadRequest("Token is required");
            }

            var command=new RevokeRefreshTokenCommand(refreshToken);
            var result = await _sender.Send(command);

            if (!result.IsSuccess)
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

            var command = new RevokeRefreshTokenCommand(refreshToken = refreshToken);
            var result = await _sender.Send(command);

            if (!result.IsSuccess)
            {
                return BadRequest("Logout failed");
            }

            Response.Cookies.Delete("refreshToken");

            return Ok("Logged out successfully");
        }

        [HttpPost("forget-password")]
        public async Task<IActionResult> ForgetPassword(ForgetPasswordCommand command)
        {
            var result = await _sender.Send(command);

            if (!result.IsSuccess)
            {
                return BadRequest(result.Message);
            }

            return Ok(result.Message);
        }

        [HttpPost("verify-otp")]
        public async Task<IActionResult> VerifyOtp(VerifyOtpCommand command)
        {
            var result = await _sender.Send(command);

            if (!result.IsSuccess)
            {
                return BadRequest(result.Message);
            }
            
            return Ok(result);
        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword(ResetPasswordCommand command)
        {
            var result = await _sender.Send(command);

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
