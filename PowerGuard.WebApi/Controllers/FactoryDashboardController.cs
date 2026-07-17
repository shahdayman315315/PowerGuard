using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PowerGuard.Application.Features.FactoryManagerDashboard.Queries.GetAllDepartmentsSummary;
using PowerGuard.Application.Features.FactoryManagerDashboard.Queries.GetFactoryDailySummary;
using PowerGuard.Application.Features.FactoryManagerDashboard.Queries.GetFactoryHourlyChart;
using PowerGuard.Application.Interfaces;

namespace PowerGuard.WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles= "FactoryManager")]
    public class FactoryDashboardController : ControllerBase
    {
        private readonly ISender _sender;

        public FactoryDashboardController(ISender sender)
        {
            _sender= sender;
        }

        [HttpGet("{factoryId}/daily-summary")]
        public async Task<IActionResult> GetDailySummary(int factoryId)
        {
            var result = await _sender.Send(new GetFactoryDailySummaryQuery(factoryId));

            if (!result.IsSuccess)
                return StatusCode(result.StatusCode, result.Message);

            return Ok(result.Data);
        }

        [HttpGet("{factoryId}/departments-summary")]
        public async Task<IActionResult> GetAllDepartmentsSummary(int factoryId)
        {
            var result = await _sender.Send(new GetAllDepartmentsSummaryQuery(factoryId));

            if (!result.IsSuccess)
                return StatusCode(result.StatusCode, result.Message);

            return Ok(result.Data);
        }

        [HttpGet("{factoryId}/hourly-chart")]
        public async Task<IActionResult> GetHourlyChart(int factoryId)
        {
            var result = await _sender.Send(new GetFactoryHourlyChartQuery(factoryId));

            if (!result.IsSuccess)
                return StatusCode(result.StatusCode, result.Message);

            return Ok(result.Data);
        }
    }
}

