using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PowerGuard.Application.Features.Notifications.Commands.DeleteAll;
using PowerGuard.Application.Features.Notifications.Commands.MarkAllAsRead;
using PowerGuard.Application.Features.Notifications.Commands.MarkAsRead;
using PowerGuard.Application.Features.Notifications.Queries.GetUnReadCount;
using PowerGuard.Application.Features.Notifications.Queries.GetUserNotifications;
using PowerGuard.Application.Interfaces;
using System.Security.Claims;

namespace PowerGuard.WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class NotificationsController : ControllerBase
    {
        private readonly ISender _sender;
        public NotificationsController(ISender sender)
        {
            _sender = sender;
        }


        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            var userId=User.Claims.FirstOrDefault(c=>c.Type==ClaimTypes.NameIdentifier)?.Value;
            
            var result=await _sender.Send(new GetUserNotificationsQuery(userId!,pageNumber,pageSize));

            return Ok(result.Data);
        }


        [HttpGet("unread-count")]
        public async Task<IActionResult> GetUnredCount()
        {
            var userId=User.Claims.FirstOrDefault(c=>c.Type==ClaimTypes.NameIdentifier)?.Value; 

            var result=await _sender.Send(new GetUnReadCountQuery(userId!));

            return Ok(result.Data);
        }


        [HttpPatch("mark-as-read/{id}")]
        public async Task<IActionResult> MarkAsRead(int id)
        {
            var userId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;

            var result=await _sender.Send(new MarkAsReadCommand(id,userId!));

            if (!result.IsSuccess)
            {
               return StatusCode(result.StatusCode, result.Message);
            }

            return Ok(result.Data);
        }


        [HttpPatch("mark-all-as-read")]
        public async Task<IActionResult> MarkAllAsRead()
        {
            var userId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;

            var result = await _sender.Send(new MarkAllAsReadCommand(userId!));

            if (!result.IsSuccess)
            {
                return StatusCode(result.StatusCode, result.Message);
            }

            return Ok(result.Data);
        }

        [HttpDelete("delete-all")]
        public async Task<IActionResult> DeleteAll()
        {
            var userId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;

            var result = await _sender.Send(new DeleteAllCommand(userId!));

            if (!result.IsSuccess)
            {
                return StatusCode(result.StatusCode, result.Message);
            }

            return Ok(result.Data);
        }
    }
}
