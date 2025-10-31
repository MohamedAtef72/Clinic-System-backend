using Clinic_System.Application.DTO;
using Clinic_System.Application.Interfaces;
using Clinic_System.Domain.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace Clinic_System.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class DoctorAvailabilityController : ControllerBase
    {
        private readonly IDoctorAvailabilityService _availabilityService;
        private readonly ILogger<DoctorAvailabilityController> _logger;

        public DoctorAvailabilityController(
            IDoctorAvailabilityService availabilityService,
            ILogger<DoctorAvailabilityController> logger)
        {
            _availabilityService = availabilityService ?? throw new ArgumentNullException(nameof(availabilityService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [HttpGet("GetAll")]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var availabilities = await _availabilityService.GetAllAvailabilitiesAsync();

                if (availabilities == null || !availabilities.Any())
                {
                    return Ok(new
                    {
                        Message = "No availabilities found",
                        Data = new List<object>()
                    });
                }

                return Ok(new
                {
                    Message = "Availabilities retrieved successfully",
                    Data = availabilities,
                    Count = availabilities.Count()
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all doctor availabilities");
                return StatusCode(500, new
                {
                    Message = "An error occurred while retrieving availabilities"
                });
            }
        }

        [HttpGet("doctor/{doctorId}")]
        public async Task<IActionResult> GetByDoctor([Required] Guid doctorId)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                if (doctorId == Guid.Empty)
                    return BadRequest(new { Message = "Doctor ID cannot be empty" });

                var availabilities = await _availabilityService.GetAvailabilitiesByDoctorAsync(doctorId);

                return Ok(new
                {
                    Message = "Doctor availabilities retrieved successfully",
                    Data = availabilities,
                    DoctorId = doctorId,
                    Count = availabilities?.Count() ?? 0
                });
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid doctor ID provided: {DoctorId}", doctorId);
                return BadRequest(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving availabilities for doctor: {DoctorId}", doctorId);
                return StatusCode(500, new
                {
                    Message = "An error occurred while retrieving doctor availabilities"
                });
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById([Range(1, int.MaxValue, ErrorMessage = "ID must be a positive number")] int id)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var availability = await _availabilityService.GetAvailabilityByIdAsync(id);

                if (availability == null)
                    return NotFound(new { Message = $"Doctor availability with ID {id} not found" });

                return Ok(new
                {
                    Message = "Doctor availability retrieved successfully",
                    Data = availability
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving doctor availability with ID: {Id}", id);
                return StatusCode(500, new
                {
                    Message = "An error occurred while retrieving the availability"
                });
            }
        }
        [HttpPost("Add")]
        public async Task<IActionResult> AddAvailability([FromBody][Required] DoctorAvailabilityCreateDTO dto)
        {
            try
            {
                if (dto == null)
                    return BadRequest(new { Message = "Availability data is required" });

                if (!ModelState.IsValid)
                    return BadRequest(new { Message = "Invalid data", Errors = ModelState });

                if (dto.StartTime >= dto.EndTime)
                    return BadRequest(new { Message = "Start time must be before end time" });

                if (dto.DoctorId == Guid.Empty)
                    return BadRequest(new { Message = "Valid doctor ID is required" });

                await _availabilityService.AddAvailabilityAsync(dto);

                return Ok(new { Message = "Doctor availability (including recurrence if any) added successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding availability");
                return StatusCode(500, new { Message = ex.Message });
            }
        }


        [HttpPut("Update/{id}")]
        public async Task<IActionResult> Update(
            [Range(1, int.MaxValue, ErrorMessage = "ID must be a positive number")] int id,
            [FromBody][Required] DoctorAvailabilityCreateDTO dto)
        {
            try
            {
                if (dto == null)
                    return BadRequest(new { Message = "Update data is required" });

                if (!ModelState.IsValid)
                    return BadRequest(new { Message = "Invalid update data", Errors = ModelState });

                // Check if availability exists
                var existingAvailability = await _availabilityService.GetAvailabilityByIdAsync(id);
                if (existingAvailability == null)
                    return NotFound(new { Message = $"Doctor availability with ID {id} not found" });

                // Business logic validation
                if (dto.StartTime >= dto.EndTime)
                    return BadRequest(new { Message = "Start time must be before end time" });

                // Validate time ranges
                if (dto.StartTime.TimeOfDay < TimeSpan.FromHours(6) || dto.EndTime.TimeOfDay > TimeSpan.FromHours(23))
                    return BadRequest(new { Message = "Availability must be within reasonable hours (6 AM - 11 PM)" });

                await _availabilityService.UpdateAvailabilityAsync(id, dto.StartTime, dto.EndTime);

                _logger.LogInformation("Doctor availability updated successfully: {Id}", id);
                return Ok(new { Message = "Doctor availability updated successfully" });
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid argument while updating availability: {Id}", id);
                return BadRequest(new { Message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Operation conflict while updating availability: {Id}", id);
                return Conflict(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating doctor availability: {Id}", id);
                return StatusCode(500, new
                {
                    Message = "An error occurred while updating the availability"
                });
            }
        }

        [HttpDelete("Delete/{id}")]
        public async Task<IActionResult> Delete([Range(1, int.MaxValue, ErrorMessage = "ID must be a positive number")] int id)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                // Check if availability exists
                var existingAvailability = await _availabilityService.GetAvailabilityByIdAsync(id);
                if (existingAvailability == null)
                    return NotFound(new { Message = $"Doctor availability with ID {id} not found" });

                await _availabilityService.DeleteAvailabilityAsync(id);

                _logger.LogInformation("Doctor availability deleted successfully: {Id}", id);
                return Ok(new { Message = "Doctor availability deleted successfully" });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Operation conflict while deleting availability: {Id}", id);
                return BadRequest(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting doctor availability: {Id}", id);
                return StatusCode(500, new
                {
                    Message = "An error occurred while deleting the availability"
                });
            }
        }
    }
}