using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PowerGuard.Application.Interfaces;
using System.Security.Claims;

namespace PowerGuard.WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class NotificationsController : ControllerBase
    {
        private readonly INotificationService _notificationService;
        public NotificationsController(INotificationService notificationService)
        {
            _notificationService = notificationService;
        }


        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            var userId=User.Claims.FirstOrDefault(c=>c.Type==ClaimTypes.NameIdentifier)?.Value;
            
            var result=await _notificationService.GetUserNotificationAsync(userId,pageNumber,pageSize);

            return Ok(result.Data);
        }


        [HttpGet("unread-count")]
        public async Task<IActionResult> GetUnredCount()
        {
            var userId=User.Claims.FirstOrDefault(c=>c.Type==ClaimTypes.NameIdentifier)?.Value; 

            var result=await _notificationService.GetUnReadCountAsync(userId);

            return Ok(result.Data);
        }


        [HttpPatch("mark-as-read/{id}")]
        public async Task<IActionResult> MarkAsRead(int id)
        {
            var userId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;

            var result=await _notificationService.MarkAsReadAsync(id,userId);

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

            var result = await _notificationService.DeleteAll(userId);

            if (!result.IsSuccess)
            {
                return StatusCode(result.StatusCode, result.Message);
            }

            return Ok(result.Data);
        }
    }
}
