using Clinic_System.Application.DTO;
using Clinic_System.Application.Interfaces;
using Clinic_System.Domain.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Clinic_System.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class DoctorAvailabilityController : ControllerBase
    {
        private readonly IDoctorAvailabilityService _availabilityService;

        public DoctorAvailabilityController(IDoctorAvailabilityService availabilityService)
        {
            _availabilityService = availabilityService;
        }

        [HttpGet("GetAll")]
        public async Task<IActionResult> GetAll()
        {
            var availabilities = await _availabilityService.GetAllAvailabilitiesAsync();
            return Ok(new { Message = "Availabilities retrieved successfully", Data = availabilities });
        }

        [HttpGet("doctor/{doctorId}")]
        public async Task<IActionResult> GetByDoctor(Guid doctorId)
        {
            var availabilities = await _availabilityService.GetAvailabilitiesByDoctorAsync(doctorId);
            return Ok(new { Message = "Availabilities retrieved successfully", Data = availabilities });
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var availability = await _availabilityService.GetAvailabilityByIdAsync(id);
            if (availability == null) return NotFound();

            return Ok(new { Message = "Availability retrieved successfully", Data = availability });
        }

        [HttpPost("Add")]
        public async Task<IActionResult> AddAvailability([FromBody] DoctorAvailabilityCreateDTO dto)
        {
            await _availabilityService.AddAvailabilityAsync(dto);
            return Ok(new { Message = "Availability added successfully" });
        }

        [HttpPut("Update/{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] DoctorAvailabilityCreateDTO dto)
        {
            await _availabilityService.UpdateAvailabilityAsync(id, dto.StartTime, dto.EndTime);
            return Ok(new { Message = "Availability updated successfully" });
        }

        [HttpDelete("Delete/{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            await _availabilityService.DeleteAvailabilityAsync(id);
            return Ok(new { Message = "Availability deleted successfully" });
        }
    }
}