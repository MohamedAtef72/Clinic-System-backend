using Clinic_System.Application.DTO;
using Clinic_System.Application.Interfaces;
using Clinic_System.Domain.Models;
using Clinic_System.Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace Clinic_System.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class AppointmentController : ControllerBase
    {
        private readonly IAppointmentService _service;

        public AppointmentController(IAppointmentService service)
        {
            _service = service;
        }

        [HttpGet("GetAll")]
        public async Task<IActionResult> GetAllAppointments()
        {
            try
            {
                var appointments = await _service.GetAllAppointmentsAsync();

                if (appointments == null || !appointments.Any())
                    return Ok(new { Message = "No appointments found.", Data = new List<object>() });

                return Ok(new { Message = "Appointments retrieved successfully.", Data = appointments });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "An error occurred while retrieving appointments", Error = ex.Message });
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetAppointmentById([Range(1, int.MaxValue, ErrorMessage = "ID must be a positive number")] int id)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var appointment = await _service.GetAppointmentByIdAsync(id);

                if (appointment == null)
                    return NotFound(new { Message = $"Appointment with ID {id} not found" });

                return Ok(new { Message = "Appointment retrieved successfully.", Data = appointment });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "An error occurred while retrieving the appointment", Error = ex.Message });
            }
        }

        [HttpPost("Create")]
        public async Task<IActionResult> CreateAppointment([FromBody][Required] CreateAppointmentDTO dto)
        {
            try
            {
                if (dto == null)
                    return BadRequest(new { Message = "Appointment data is required" });

                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                await _service.CreateAppointmentAsync(dto);

                return Ok(
                    new { Message = "Appointment created successfully."}
                );
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "An error occurred while creating the appointment", Error = ex.Message });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateAppointmentStatus(
            [Range(1, int.MaxValue, ErrorMessage = "ID must be a positive number")] int id,
            [FromBody][Required] UpdateAppointmentDTO dto)
        {
            try
            {
                if (dto == null)
                    return BadRequest(new { Message = "Update data is required" });

                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                // Check if appointment exists before updating
                var existingAppointment = await _service.GetAppointmentByIdAsync(id);
                if (existingAppointment == null)
                    return NotFound(new { Message = $"Appointment with ID {id} not found" });

                dto.Id = id;
                await _service.UpdateAppointmentStatusAsync(dto);

                return Ok(new { Message = "Appointment updated successfully." });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "An error occurred while updating the appointment", Error = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAppointment([Range(1, int.MaxValue, ErrorMessage = "ID must be a positive number")] int id)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                // Check if appointment exists before deleting
                var existingAppointment = await _service.GetAppointmentByIdAsync(id);
                if (existingAppointment == null)
                    return NotFound(new { Message = $"Appointment with ID {id} not found" });

                await _service.DeleteAppointmentAsync(id);

                return Ok(new { Message = "Appointment deleted successfully." });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "An error occurred while deleting the appointment", Error = ex.Message });
            }
        }

        [HttpGet("doctor/{doctorId}")]
        public async Task<IActionResult> GetAppointmentsByDoctorId([Required] Guid doctorId)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                if (doctorId == Guid.Empty)
                    return BadRequest(new { Message = "Doctor ID cannot be empty" });

                var result = await _service.GetAppointmentsByDoctorIdAsync(doctorId);

                return Ok(new
                {
                    Message = "Appointments retrieved successfully.",
                    Data = result
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "An error occurred while retrieving appointments", Error = ex.Message });
            }
        }

        [HttpGet("patient/{patientId}")]
        public async Task<IActionResult> GetAppointmentsByPatientId([Required] Guid patientId)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                if (patientId == Guid.Empty)
                    return BadRequest(new { Message = "Patient ID cannot be empty" });

                var result = await _service.GetAppointmentsByPatientIdAsync(patientId);

                return Ok(new
                {
                    Message = "Appointments retrieved successfully.",
                    Data = result
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "An error occurred while retrieving appointments", Error = ex.Message });
            }
        }
    }
}