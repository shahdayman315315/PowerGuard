using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PowerGuard.Application.Interfaces;

namespace PowerGuard.WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles ="DepartmentManager")]
    public class DepartmentDashboardController : ControllerBase
    {
        private readonly IDepartmentDashboardService _departmentDashboardService;

        public DepartmentDashboardController(IDepartmentDashboardService departmentDashboardService)
        {
            _departmentDashboardService = departmentDashboardService;
        }

        [HttpGet("daily-summary/{departmentId}")]
        public async Task<IActionResult> GetDailySummary(int departmentId)
        {
            var result = await _departmentDashboardService.GetDepartmentDailySummaryAsync(departmentId);

            if (!result.IsSuccess)
                return StatusCode(result.StatusCode, result.Message);

            return Ok(result.Data);
        }

        [HttpGet("hourly-chart/{departmentId}")]
        public async Task<IActionResult> GetHourlyChart(int departmentId)
        {
            var result = await _departmentDashboardService.GetDepartmentHourlyChartAsync(departmentId);

            if (!result.IsSuccess)
                return StatusCode(result.StatusCode, result.Message);

            return Ok(result.Data);
        }
    }
}

