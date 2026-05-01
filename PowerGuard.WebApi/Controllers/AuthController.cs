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
        private readonly IMediator _mediator;
        private readonly IMapper _mapper;
        public AuthController(IMediator mediator,IMapper mapper)
        {
            _mediator = mediator;
            _mapper = mapper;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterDto registerDto)
        {
            var command=_mapper.Map<RegisterCommand>(registerDto);  

            var result=await _mediator.Send(command);

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
        public async Task<IActionResult> Login(LoginDto dto)
        {
            var command=_mapper.Map<LoginCommand>(dto);
            var result = await _mediator.Send(command);

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

            var command=_mapper.Map<RefreshTokenCommand>(dto);
            var result = await _mediator.Send(command);

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

            var command=new RevokeRefreshTokenCommand(refreshToken=refreshToken);
            var result = await _mediator.Send(command);

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
            var result = await _mediator.Send(command);

            if (!result.IsSuccess)
            {
                return BadRequest("Logout failed");
            }

            Response.Cookies.Delete("refreshToken");

            return Ok("Logged out successfully");
        }

        [HttpPost("forget-password")]
        public async Task<IActionResult> ForgetPassword(ForgetPasswordDto dto)
        {
            var command=new ForgetPasswordCommand(dto.Email);
            var result = await _mediator.Send(command);

            if (!result.IsSuccess)
            {
                return BadRequest(result.Message);
            }

            return Ok(result.Message);
        }

        [HttpPost("verify-otp")]
        public async Task<IActionResult> VerifyOtp(VerifyOtpDto dto)
        {
            var command=_mapper.Map<VerifyOtpCommand>(dto);
            var result = await _mediator.Send(command);

            if (!result.IsSuccess)
            {
                return BadRequest(result.Message);
            }
            
            return Ok(result);
        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword(ResetPasswordDto dto)
        {
            var command=_mapper.Map<ResetPasswordCommand>(dto);
            var result = await _mediator.Send(command);

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
