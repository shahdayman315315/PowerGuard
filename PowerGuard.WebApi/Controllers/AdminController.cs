using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PowerGuard.Application.Dtos;
using PowerGuard.Application.Interfaces;

namespace PowerGuard.WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class AdminController : ControllerBase
    {
        private readonly IFactoryService _factoryService;
        private readonly IAdminService _adminService;
        public AdminController(IFactoryService factoryService, IAdminService adminService)
        {
            _factoryService = factoryService;
            _adminService = adminService;
        }

        [HttpGet("admin-dashboard")]    
        public async Task<IActionResult> GetAdminDashboard()
        {
            var result = await _adminService.GetAdminDashboardStats();

            if (!result.IsSuccess)
            {
                return BadRequest(result.Message);
            }

            return Ok(result);
        }


        [HttpGet("factories")]
        public async Task<IActionResult> GetAllFactories()
        {
            var result = await _factoryService.GetAllFactories();

            if (!result.IsSuccess)
            {
                return BadRequest(result.Message);
            }

            return Ok(result.Data);
        }


        [HttpGet("pending-factories")]
        public async Task<IActionResult> GetPendingFactories()
        {
            var result = await _factoryService.GetPendingFactories();

            if (!result.IsSuccess)
            {
                return BadRequest(result.Message);
            }

            return Ok(result.Data);
        }

        [HttpGet("active-factories")]
        public async Task<IActionResult> GetActiveFactories()
        {
            var result = await _factoryService.GetActiveFactories();

            if (!result.IsSuccess)
            {
                return BadRequest(result.Message);
            }

            return Ok(result.Data);
        }


        [HttpPut("review-factory")]
        public async Task<IActionResult> ReviewFactory(ReviewFactoryDto dto)
        {
            var result = await _adminService.ReviewFactory(dto);
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


        [HttpDelete("delete-factory/{id}")]
        public async Task<IActionResult> DeleteFactory(int id)
        {
            var result = await _factoryService.DeleteFactory(id);

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

    }
}
