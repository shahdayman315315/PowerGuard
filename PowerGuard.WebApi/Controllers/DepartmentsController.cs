using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PowerGuard.Application.Dtos;
using PowerGuard.Application.Interfaces;
using System.Threading.Tasks;

namespace PowerGuard.WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles ="Admin,FactoryManager")]
    public class DepartmentsController : ControllerBase
    {
        private readonly IDepartmentService _departmentsService;
        public DepartmentsController(IDepartmentService departmentsService)
        {
            _departmentsService = departmentsService;
        }


        [HttpPost("Create")]
        public async Task<IActionResult> Create(CreateDepartmentDto dto)
        {
            var result = await _departmentsService.CreateDepartmentAsync(dto);

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


        [HttpPost("Add-DepartmentManager")]
        public async Task<IActionResult> AddDepartmentManager(RegisterManagerDto dto)
        {
            var result= await _departmentsService.RegisterDepartmentManager(dto);

            if (!result.IsSuccess)
            {
                if (result.StatusCode == 404)
                {
                    return NotFound(result.Message);
                }

                return BadRequest(result.Message);
            }

            return Ok(result.Message);
        }


        [HttpGet("AvailableDepartmentManagers")]
        public async Task<IActionResult> GetAvailableManagers()
        {
            var result = await _departmentsService.GetAvailableManagersAsync();

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


        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var result=await _departmentsService.GetAllAsync();

            if (!result.IsSuccess)
            {
                return BadRequest(result.Message);
            }

            return Ok(result.Data);
        }


        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var result=await _departmentsService.GetByIdAsync(id);

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


        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, UpdateDepartmentDto dto)
        {
            var result= await _departmentsService.UpdateAsync(dto, id);

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


        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var result= await _departmentsService.DeleteAsync(id);

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
