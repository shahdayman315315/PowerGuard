using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PowerGuard.Application.Interfaces;
using System.Security.Claims;

namespace PowerGuard.WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class NotificationsController : ControllerBase
    {
        private readonly INotificationService _notificationService;
        public NotificationsController(INotificationService notificationService)
        {
            _notificationService = notificationService;
        }


        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var userId=User.Claims.FirstOrDefault(c=>c.Type==ClaimTypes.NameIdentifier)?.Value;
            
            var result=await _notificationService.GetUserNotificationAsync(userId);

            return Ok(result.Data);
        }


        [HttpGet("unread-count")]
        public async Task<IActionResult> GetUnredCount()
        {
            var userId=User.Claims.FirstOrDefault(c=>c.Type==ClaimTypes.NameIdentifier)?.Value; 

            var result=await _notificationService.GetUnReadCountAsync(userId);

            return Ok(result.Data);
        }


        [HttpPatch("{id}")]
        public async Task<IActionResult> MarkAsRead(int id)
        {
            var userId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;

            var result=await _notificationService.MarkAsReadAsync(id,userId);

            if (!result.IsSuccess)
            {
                if (result.StatusCode == 404)
                {
                    return NotFound(result.Message);
                }

                return BadRequest(result.Message);
            }

            return Ok(result.Data);
        }


        [HttpDelete]
        public async Task<IActionResult> DeleteAll()
        {
            var userId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;

            var result = await _notificationService.DeleteAll(userId);

            if (!result.IsSuccess)
            {
                return BadRequest(result.Message);
            }

            return Ok(result.Data);
        }
    }
}
