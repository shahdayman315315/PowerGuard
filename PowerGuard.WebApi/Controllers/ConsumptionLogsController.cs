using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PowerGuard.Application.Dtos;
using PowerGuard.Application.Interfaces;

namespace PowerGuard.WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ConsumptionLogsController : ControllerBase
    {
        private readonly IConsumptionService _consumptionService;
        public ConsumptionLogsController(IConsumptionService consumptionService)
        {
            _consumptionService = consumptionService;
        }


        [HttpPost]
        public async Task<IActionResult> EnterConsumption(ConsumptionLogDto dto)
        {
            var result =await _consumptionService.EnterConsumptionAsync(dto);

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
