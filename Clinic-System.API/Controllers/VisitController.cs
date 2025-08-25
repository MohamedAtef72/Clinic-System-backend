using Clinic_System.Application.DTO;
using Clinic_System.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

namespace Clinic_System.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class VisitController : ControllerBase
    {
        private readonly IVisitService _visitService;

        public VisitController(IVisitService visitService)
        {
            _visitService = visitService;
        }

        [HttpGet("GetAll")]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var visits = await _visitService.GetAllVisitsAsync();
                return Ok(new { Message = "Visits retrieved successfully", Data = visits });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "An error occurred while retrieving visits", Error = ex.Message });
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById([Range(1, int.MaxValue, ErrorMessage = "ID must be a positive number")] int id)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var visit = await _visitService.GetVisitByIdAsync(id);
                if (visit == null)
                    return NotFound(new { Message = $"Visit with ID {id} not found" });

                return Ok(new { Message = "Visit retrieved successfully", Data = visit });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "An error occurred while retrieving the visit", Error = ex.Message });
            }
        }

        [HttpGet("doctor/{doctorId}")]
        public async Task<IActionResult> GetByDoctorId([Required] Guid doctorId)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                if (doctorId == Guid.Empty)
                    return BadRequest(new { Message = "Doctor ID cannot be empty" });

                var visits = await _visitService.GetVisitsByDoctorIdAsync(doctorId);
                return Ok(new { Message = "Visits retrieved successfully", Data = visits });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "An error occurred while retrieving visits", Error = ex.Message });
            }
        }

        [HttpGet("patient/{patientId}")]
        public async Task<IActionResult> GetByPatientId([Required] Guid patientId)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                if (patientId == Guid.Empty)
                    return BadRequest(new { Message = "Patient ID cannot be empty" });

                var visits = await _visitService.GetVisitsByPatientIdAsync(patientId);
                return Ok(new { Message = "Visits retrieved successfully", Data = visits });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "An error occurred while retrieving visits", Error = ex.Message });
            }
        }

        [HttpPost("Create")]
        public async Task<IActionResult> Create([FromBody][Required] VisitCreateDTO visitDto)
        {
            try
            {
                if (visitDto == null)
                    return BadRequest(new { Message = "Visit data is required" });

                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                await _visitService.AddVisitAsync(visitDto);
                return Ok(
                    new { Message = "Visit created successfully"}
                );
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "An error occurred while creating the visit", Error = ex.Message });
            }
        }

        [HttpPut("Update/{id}")]
        public async Task<IActionResult> Update(
            [Range(1, int.MaxValue, ErrorMessage = "ID must be a positive number")] int id,
            [FromBody][Required] VisitReadDTO visitDto)
        {
            try
            {
                if (visitDto == null)
                    return BadRequest(new { Message = "Visit data is required" });

                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                if (id != visitDto.Id)
                    return BadRequest(new { Message = "URL ID does not match the visit ID in the request body" });

                // Check if visit exists before updating
                var existingVisit = await _visitService.GetVisitByIdAsync(id);
                if (existingVisit == null)
                    return NotFound(new { Message = $"Visit with ID {id} not found" });

                await _visitService.UpdateVisitAsync(visitDto);
                return Ok(new { Message = "Visit updated successfully" });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "An error occurred while updating the visit", Error = ex.Message });
            }
        }

        [HttpDelete("Delete/{id}")]
        public async Task<IActionResult> Delete([Range(1, int.MaxValue, ErrorMessage = "ID must be a positive number")] int id)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                // Check if visit exists before deleting
                var existingVisit = await _visitService.GetVisitByIdAsync(id);
                if (existingVisit == null)
                    return NotFound(new { Message = $"Visit with ID {id} not found" });

                await _visitService.DeleteVisitAsync(id);
                return Ok(new { Message = "Visit deleted successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "An error occurred while deleting the visit", Error = ex.Message });
            }
        }
    }
}