using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PowerGuard.Application.Dtos;
using PowerGuard.Application.Features.Department.Commands.UpdateConsumptionLimit;
using PowerGuard.Application.Features.Factory.CreateFactory;
using PowerGuard.Application.Features.Factory.Queries.GetFactory;
using PowerGuard.Application.Features.Factory.UpdateFactory;
using PowerGuard.Application.Interfaces;
using System.Security.Claims;

namespace PowerGuard.WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin,FactoryManager")]
    public class FactoriesController : ControllerBase
    {
        private readonly ISender _sender;
        public FactoriesController(ISender sender)
        {
            _sender = sender;
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateFactoryCommand command)
        {

            var result = await _sender.Send(command);

            if (!result.IsSuccess)
            {
                return BadRequest(result.Message);
            }

            return Ok(result.Data);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _sender.Send(new GetFactoryQuery(id));

            if (!result.IsSuccess)
            {
                return NotFound(result.Message);
            }

            return Ok(result.Data);
        }



        [HttpPut("update")]
        public async Task<IActionResult> Update(UpdateFactoryCommand command)
        {
            var result = await _sender.Send(command);

            if (!result.IsSuccess)
            {
                return StatusCode(result.StatusCode, result.Message);
            }

            return Ok(result.Data);
        }


        [HttpPatch("Update-ConsumptionLimit")]
        public async Task<IActionResult> UpdateConsumptionLimit(UpdateConsumptionLimitCommand command)
        {

            var result =await _sender.Send(command);

            if(!result.IsSuccess)
            {
                return StatusCode(result.StatusCode, result.Message);
            }


            return Ok(result.Data);
        }
        
    }
}
