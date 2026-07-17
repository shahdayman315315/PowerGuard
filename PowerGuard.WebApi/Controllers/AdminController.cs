using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PowerGuard.Application.Dtos;
using PowerGuard.Application.Features.Admin.Commands.activateFactory;
using PowerGuard.Application.Features.Admin.Commands.ReviewFactory;
using PowerGuard.Application.Features.Admin.Queries.GetAdminDashboardStats;
using PowerGuard.Application.Features.Factory.DeleteFactory;
using PowerGuard.Application.Features.Factory.Queries.GetActiveFactories;
using PowerGuard.Application.Features.Factory.Queries.GetAllFactories;
using PowerGuard.Application.Features.Factory.Queries.GetPendingFactories;
using PowerGuard.Application.Interfaces;

namespace PowerGuard.WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class AdminController : ControllerBase
    {
        private readonly ISender _sender;
        public AdminController(ISender sender)
        {
            _sender = sender;
        }

        [HttpGet("admin-dashboard")]    
        public async Task<IActionResult> GetAdminDashboard()
        {
            var result = await _sender.Send(new GetAdminDashboardStatsQuery());

            if (!result.IsSuccess)
            {
                return BadRequest(result.Message);
            }

            return Ok(result);
        }


        [HttpGet("factories")]
        public async Task<IActionResult> GetAllFactories()
        {
            var result = await _sender.Send(new GetAllFactoriesQuery());

            if (!result.IsSuccess)
            {
                return BadRequest(result.Message);
            }

            return Ok(result.Data);
        }


        [HttpGet("pending-factories")]
        public async Task<IActionResult> GetPendingFactories()
        {
            var result = await _sender.Send(new GetPendingFactoriesQuery());

            if (!result.IsSuccess)
            {
                return BadRequest(result.Message);
            }

            return Ok(result.Data);
        }

        [HttpGet("active-factories")]
        public async Task<IActionResult> GetActiveFactories()
        {
            var result = await _sender.Send(new GetActiveFactoriesQuery());

            if (!result.IsSuccess)
            {
                return BadRequest(result.Message);
            }

            return Ok(result.Data);
        }


        [HttpPut("review-factory")]
        public async Task<IActionResult> ReviewFactory(ReviewFactoryCommand command)
        {
            var result = await _sender.Send(command); 

            if (!result.IsSuccess)
                return StatusCode(result.StatusCode, result.Message);

            return Ok(result.Data);
        }



        [HttpPut("activate-factory/{id}")]
        public async Task<IActionResult> ActivateFactory(int id)
        {
            var result =await _sender.Send(new ActivateFactoryCommand(id));

            if(!result.IsSuccess)
            {
                return BadRequest(result.Message);
            }

            return Ok(result.Message);
        }


        [HttpDelete("delete-factory/{id}")]
        public async Task<IActionResult> DeleteFactory(int id)
        {
            var result = await _sender.Send(new DeleteFactoryCommand(id));

            if (!result.IsSuccess)
                return StatusCode(result.StatusCode, result.Message);

            return Ok(result.Data);
        }

    }
}
