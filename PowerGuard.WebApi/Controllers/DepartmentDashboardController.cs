using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PowerGuard.Application.Features.DepartmentManagerDashboard.Queries.GetDepartmentDailySummary;
using PowerGuard.Application.Features.DepartmentManagerDashboard.Queries.GetDepartmentHourlyChart;
using PowerGuard.Application.Interfaces;

namespace PowerGuard.WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles ="DepartmentManager")]
    public class DepartmentDashboardController : ControllerBase
    {
        private readonly ISender _sender;

        public DepartmentDashboardController(ISender sender)
        {
            _sender = sender;
        }

        [HttpGet("daily-summary/{departmentId}")]
        public async Task<IActionResult> GetDailySummary(int departmentId)
        {
            var result = await _sender.Send(new GetDepartmentDailySummaryQuery(departmentId));

            if (!result.IsSuccess)
                return StatusCode(result.StatusCode, result.Message);

            return Ok(result.Data);
        }

        [HttpGet("hourly-chart/{departmentId}")]
        public async Task<IActionResult> GetHourlyChart(int departmentId)
        {
            var result = await _sender.Send(new GetDepartmentHourlyChartQuery(departmentId));

            if (!result.IsSuccess)
                return StatusCode(result.StatusCode, result.Message);

            return Ok(result.Data);
        }
    }
}

