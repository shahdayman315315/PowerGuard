using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PowerGuard.Application.Dtos;
using PowerGuard.Application.Interfaces;
using System.Security.Claims;

namespace PowerGuard.WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin,FactoryManager")]
    public class FactoriesController : ControllerBase
    {
        private readonly IFactoryService _factoryService;
        public FactoriesController(IFactoryService factoryService)
        {
            _factoryService = factoryService;
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateFactoryDto dto)
        {
            var userId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;

            var result = await _factoryService.CreateFactory(dto, userId);

            if (!result.IsSuccess)
            {
                return BadRequest(result.Message);
            }

            return Ok(result.Data);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _factoryService.GetFactoryById(id);

            if (!result.IsSuccess)
            {
                return NotFound(result.Message);
            }

            return Ok(result.Data);
        }



        [HttpPut("update/{id}")]
        public async Task<IActionResult> Update(int id, UpdateFactoryDto dto)
        {
            var result = await _factoryService.UpdateFactory(id, dto);

            if (!result.IsSuccess)
            {
                return StatusCode(result.StatusCode, result.Message);
            }

            return Ok(result.Data);
        }


        [HttpPatch("Update-ConsumptionLimit/{factoryid}")]
        public async Task<IActionResult> UpdateConsumptionLimit(int factoryid, UpdateConsumptionLimitDto dto)
        {
            var userId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;

            var result =await _factoryService.UpdateConsumptionLimit(factoryid, dto,userId);

            if(!result.IsSuccess)
            {
                return StatusCode(result.StatusCode, result.Message);
            }


            return Ok(result.Data);
        }
        
    }
}
