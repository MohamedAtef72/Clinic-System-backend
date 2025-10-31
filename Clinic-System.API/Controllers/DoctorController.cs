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

        public DoctorController(IDoctorService doctorService)
        {
            _doctorService = doctorService;
        }

        [HttpGet("AllDoctors")]
        public async Task<IActionResult> GetAll([FromQuery] string? searchName, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 6 )
        {
            try
            {
                var (doctors, totalCount) = await _doctorService.GetAllDoctorsAsync( searchName,pageNumber, pageSize);

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
            var doctor = await _doctorService.GetDoctorByIdAsync(id);
            if (doctor == null) return NotFound(new { Message = "Doctor not found" });
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
