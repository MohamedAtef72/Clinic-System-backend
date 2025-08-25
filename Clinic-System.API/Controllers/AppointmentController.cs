using Clinic_System.Application.DTO;
using Clinic_System.Application.Interfaces;
using Clinic_System.Domain.Models;
using Clinic_System.Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

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
            var appointments = await _service.GetAllAppointmentsAsync();
            if(appointments == null) return NotFound(new { Message = "Appointments Not Found."});
            return Ok(new { Message = "Appointments retrieved successfully.", Data = appointments });
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetAppointmentById(int id)
        {
            var appointment = await _service.GetAppointmentByIdAsync(id);
            if (appointment == null) return NotFound();
            return Ok(new { Message = "Appointment retrieved successfully.", Data = appointment });
        }

        [HttpPost("Create")]
        public async Task<IActionResult> CreateAppointment([FromBody] CreateAppointmentDTO dto)
        {
            await _service.CreateAppointmentAsync(dto);
            return Ok(new { Message = "Appointment created successfully." });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateAppointmentStatus(int id, [FromBody] UpdateAppointmentDTO dto)
        {
            dto.Id = id;
            await _service.UpdateAppointmentStatusAsync(dto);
            return Ok(new { Message = "Appointment updated successfully." });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAppointment(int id)
        {
            await _service.DeleteAppointmentAsync(id);
            return Ok(new { Message = "Appointment deleted successfully." });
        }
        [HttpGet("doctor/{doctorId}")]
        public async Task<IActionResult> GetAppointmentsByDoctorId(Guid doctorId)
        {
            var result = await _service.GetAppointmentsByDoctorIdAsync(doctorId);
            return Ok(new { Message = "Data retrieved successfully.", Data = result });
        }

        [HttpGet("patient/{patientId}")]
        public async Task<IActionResult> GetAppointmentsByPatientId(Guid patientId)
        {
            var result = await _service.GetAppointmentsByPatientIdAsync(patientId);
            return Ok(new { Message = "Data retrieved successfully.", Data = result});
        }
    }
}
