using Clinic_System.Application.DTO;
using Clinic_System.Application.Interfaces;
using Clinic_System.Domain.Constant;
using Clinic_System.Domain.Models;
using Clinic_System.Infrastructure.Data;
using Clinic_System.Infrastructure.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Clinic_System.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class SpecialityController : ControllerBase
    {
        private readonly ISpecialityService _service;

        public SpecialityController(ISpecialityService service)
        {
            _service = service;
        }

        [HttpGet("AllSpecialities")]
        public async Task<IActionResult> GetAll()
        {
            var specialities = await _service.GetAllAsync();
            if (!specialities.Any())
                return NotFound(new { Message = "No Specialities Found" });

            return Ok(new { Message = "Specialities retrieved successfully", Data = specialities });
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var speciality = await _service.GetByIdAsync(id);
            if (speciality == null)
                return NotFound();

            return Ok(new { Message = "Speciality retrieved successfully", Data = speciality });
        }

        [HttpPost]
        [Authorize(Roles = Role.Admin)]
        public async Task<IActionResult> Create([FromBody] SpecialityInfo specialityInfo)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var speciality = new Speciality
            {
                Name = specialityInfo.Name,
            };

            var created = await _service.CreateAsync(speciality);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = Role.Admin)]
        public async Task<IActionResult> Update(int id, [FromBody] Speciality speciality)
        {
            if (id != speciality.Id)
                return BadRequest("ID mismatch");

            var updated = await _service.UpdateAsync(id, speciality);
            if (!updated)
                return NotFound();

            return NoContent();
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = Role.Admin)]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var deleted = await _service.DeleteAsync(id);
                if (!deleted) return NotFound();
                return NoContent();
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
