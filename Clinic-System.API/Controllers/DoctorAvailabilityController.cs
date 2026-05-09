using Clinic_System.Application.DTO;
using Clinic_System.Application.Interfaces;
using Clinic_System.Domain.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.RateLimiting;

namespace Clinic_System.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class DoctorAvailabilityController : ControllerBase
    {
        private readonly IDoctorAvailabilityService _availabilityService;
        private readonly ILogger<DoctorAvailabilityController> _logger;
        private readonly ICacheService _cache;

        public DoctorAvailabilityController(
            IDoctorAvailabilityService availabilityService,
            ILogger<DoctorAvailabilityController> logger,
            ICacheService cache)
        {
            _availabilityService = availabilityService;
            _logger = logger;
            _cache = cache;
        }

        [HttpGet("GetAll")]
        [EnableRateLimiting("ReadPolicy")]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var version = await _cache.GetVersionAsync("doctoravailability:all");
                var cacheKey = $"doctoravailability:all:{version}";
                var availabilities = await _cache.GetAsync<object>(cacheKey);
                if (availabilities == null)
                {
                    var fresh = await _availabilityService.GetAllAvailabilitiesAsync();
                    if (fresh == null || !fresh.Any())
                    {
                        return Ok(new { Message = "No availabilities found", Data = new List<object>() });
                    }
                    await _cache.SetAsync(cacheKey, new { Message = "Availabilities retrieved successfully", Data = fresh, Count = fresh.Count() }, TimeSpan.FromMinutes(5));
                    availabilities = new { Message = "Availabilities retrieved successfully", Data = fresh, Count = fresh.Count() };
                }
                return Ok(availabilities);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all doctor availabilities");
                return StatusCode(500, new { Message = "An error occurred while retrieving availabilities" });
            }
        }

        [HttpGet("doctor/{doctorId}")]
        [EnableRateLimiting("ReadPolicy")]
        public async Task<IActionResult> GetByDoctor([Required] Guid doctorId)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                if (doctorId == Guid.Empty)
                    return BadRequest(new { Message = "Doctor ID cannot be empty" });

                var version = await _cache.GetVersionAsync($"doctoravailability:doctor:{doctorId}");
                var cacheKey = $"doctoravailability:doctor:{doctorId}:{version}";
                var availabilities = await _cache.GetAsync<object>(cacheKey);
                if (availabilities == null)
                {
                    var fresh = await _availabilityService.GetAvailabilitiesByDoctorAsync(doctorId);
                    availabilities = new { Message = "Doctor availabilities retrieved successfully", Data = fresh, DoctorId = doctorId, Count = fresh?.Count() ?? 0 };
                    await _cache.SetAsync(cacheKey, availabilities, TimeSpan.FromMinutes(5));
                }
                return Ok(availabilities);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid doctor ID provided: {DoctorId}", doctorId);
                return BadRequest(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving availabilities for doctor: {DoctorId}", doctorId);
                return StatusCode(500, new { Message = "An error occurred while retrieving doctor availabilities" });
            }
        }

        [HttpGet("{id}")]
        [EnableRateLimiting("ReadPolicy")]
        public async Task<IActionResult> GetById([Range(1, int.MaxValue, ErrorMessage = "ID must be a positive number")] int id)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var version = await _cache.GetVersionAsync($"doctoravailability:id:{id}");
                var cacheKey = $"doctoravailability:id:{id}:{version}";
                var availability = await _cache.GetAsync<object>(cacheKey);
                if (availability == null)
                {
                    var fresh = await _availabilityService.GetAvailabilityByIdAsync(id);
                    if (fresh == null)
                        return NotFound(new { Message = $"Doctor availability with ID {id} not found" });
                    availability = new { Message = "Doctor availability retrieved successfully", Data = fresh };
                    await _cache.SetAsync(cacheKey, availability, TimeSpan.FromMinutes(5));
                }
                return Ok(availability);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving doctor availability with ID: {Id}", id);
                return StatusCode(500, new { Message = "An error occurred while retrieving the availability" });
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
                // Invalidate all related cache
                await _cache.BumpVersionAsync("doctoravailability:all");
                await _cache.BumpVersionAsync($"doctoravailability:doctor:{dto.DoctorId}");
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
                // Invalidate all related cache
                await _cache.BumpVersionAsync("doctoravailability:all");
                await _cache.BumpVersionAsync($"doctoravailability:doctor:{dto.DoctorId}");
                await _cache.BumpVersionAsync($"doctoravailability:id:{id}");
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
                return StatusCode(500, new { Message = "An error occurred while updating the availability" });
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
                // Invalidate all related cache
                await _cache.BumpVersionAsync("doctoravailability:all");
                if (existingAvailability != null)
                {
                    await _cache.BumpVersionAsync($"doctoravailability:doctor:{existingAvailability.Id}");
                }
                await _cache.BumpVersionAsync($"doctoravailability:id:{id}");
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
                return StatusCode(500, new { Message = "An error occurred while deleting the availability" });
            }
        }
    }
}