using Clinic_System.Application.DTO;
using Clinic_System.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

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
        public async Task<IActionResult> GetAll([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 6)
        {
            var (doctors, totalCount) = await _doctorService.GetAllDoctorsAsync(pageNumber, pageSize);
            return Ok(new
            {
                Message = "Doctors retrieved successfully",
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize,
                Data = doctors
            });
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(string id)
        {
            var doctor = await _doctorService.GetDoctorByIdAsync(id);
            if (doctor == null) return NotFound(new { Message = "Doctor not found" });
            return Ok(new { Message = "Doctor retrieved successfully", Data = doctor });
        }

    }
}
