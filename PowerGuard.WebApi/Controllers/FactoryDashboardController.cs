using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PowerGuard.Application.Interfaces;

namespace PowerGuard.WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles= "FactoryManager")]
    public class FactoryDashboardController : ControllerBase
    {
        private readonly IFactoryDashboardService _factoryDashboardService;

        public FactoryDashboardController(IFactoryDashboardService factoryDashboardService)
        {
            _factoryDashboardService = factoryDashboardService;
        }

        [HttpGet("{factoryId}/daily-summary")]
        public async Task<IActionResult> GetDailySummary(int factoryId)
        {
            var result = await _factoryDashboardService.GetFactoryDailySummaryAsync(factoryId);

            if (!result.IsSuccess)
                return StatusCode(result.StatusCode, result.Message);

            return Ok(result.Data);
        }

        [HttpGet("{factoryId}/departments-summary")]
        public async Task<IActionResult> GetAllDepartmentsSummary(int factoryId)
        {
            var result = await _factoryDashboardService.GetAllDepartmentsSummaryAsync(factoryId);

            if (!result.IsSuccess)
                return StatusCode(result.StatusCode, result.Message);

            return Ok(result.Data);
        }

        [HttpGet("{factoryId}/hourly-chart")]
        public async Task<IActionResult> GetHourlyChart(int factoryId)
        {
            var result = await _factoryDashboardService.GetFactoryHourlyChartAsync(factoryId);

            if (!result.IsSuccess)
                return StatusCode(result.StatusCode, result.Message);

            return Ok(result.Data);
        }
    }
}

