using Clinic_System.Application.DTO;
using Clinic_System.Application.Interfaces;
using Clinic_System.Domain.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Linq.Expressions;

namespace Clinic_System.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class DoctorController : ControllerBase
    {
        private readonly IDoctorService _doctorService;
        private readonly Clinic_System.Application.Interfaces.ICacheService _cache;

        public DoctorController(IDoctorService doctorService, Clinic_System.Application.Interfaces.ICacheService cache)
        {
            _doctorService = doctorService;
            _cache = cache;
        }

        [HttpGet("AllDoctors")]
        public async Task<IActionResult> GetAll([FromQuery] string? searchName, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 6 )
        {
            try
            {
                // Try cache first
                var version = await _cache.GetVersionAsync("doctors:list");
                var sanitizedSearch = string.IsNullOrEmpty(searchName) ? "" : searchName;
                var cacheKey = $"doctors:list:{version}:{sanitizedSearch}:{pageNumber}:{pageSize}";
                var cached = await _cache.GetAsync<Clinic_System.Application.DTO.DoctorsListDto>(cacheKey);

                List<DoctorInfoDTO> doctors;
                int totalCount;

                if (cached != null)
                {
                    doctors = cached.Doctors;
                    totalCount = cached.TotalCount;
                }
                else
                {
                    var (dList, tCount) = await _doctorService.GetAllDoctorsAsync(searchName, pageNumber, pageSize);
                    doctors = dList;
                    totalCount = tCount;

                    if (doctors != null && doctors.Any())
                    {
                        var dto = new Clinic_System.Application.DTO.DoctorsListDto { Doctors = doctors, TotalCount = totalCount };
                        await _cache.SetAsync(cacheKey, dto, TimeSpan.FromMinutes(5));
                    }
                }

                if (doctors == null || !doctors.Any())
                    return Ok(new { Message = "No doctors found.", Data = new List<object>() });

                return Ok(new
                {
                    Message = "Doctors retrieved successfully",
                    TotalCount = totalCount,
                    PageNumber = pageNumber,
                    PageSize = pageSize,
                    Data = doctors
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "An error occurred while retrieving appointments", Error = ex.Message });
            }
            }
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            // Try cache first for doctor detail
            var version = await _cache.GetVersionAsync($"doctor:{id}");
            var cacheKey = $"doctor:{id}:{version}";
            var cached = await _cache.GetAsync<DoctorInfoDTO>(cacheKey);

            DoctorInfoDTO doctor;
            if (cached != null)
            {
                doctor = cached;
            }
            else
            {
                doctor = await _doctorService.GetDoctorByIdAsync(id);
                if (doctor == null) return NotFound(new { Message = "Doctor not found" });
                await _cache.SetAsync(cacheKey, doctor, TimeSpan.FromMinutes(10));
            }

            return Ok(new { Message = "Doctor retrieved successfully", Data = doctor });
        }
        [HttpPatch("SetPrice/{id}")]
        public async Task<IActionResult> UpdatePrice(Guid id ,DoctorPriceDTO model)
        {
            if (model.Price <= 0)
                return BadRequest(new { Message = "Price must be greater than zero." });

            var result = await _doctorService.UpdateDoctorPriceAsync(id, model.Price);
            if (!result)
                return NotFound(new { Message = "Doctor not found or price not updated." });

            return Ok(new { Message = "Doctor price updated successfully." });
        }
    }
}
