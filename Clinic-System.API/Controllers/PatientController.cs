using Clinic_System.Application.Interfaces;
using Clinic_System.Domain.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Clinic_System.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class PatientController : ControllerBase
    {
        private readonly IPatientService _patientService;

        public PatientController(IPatientService patientService)
        {
            _patientService = patientService;
        }

        [HttpGet("GetAll")]
        public async Task<IActionResult> GetAll(int pageNumber = 1, int pageSize = 5)
        {
            var (patients , totalCount) = await _patientService.GetAllPatientsAsync(pageNumber,pageSize);
            if(patients == null)
            {
                return NotFound(new { Message = "Patients Not Found" });
            }
            return Ok(new {Message = "Patients Retrieved Successfully",
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize,
                Data = patients});
        }

        [HttpGet("Patient/{id}")]
        public async Task<IActionResult> GetById(string id)
        {
            var patient = await _patientService.GetPatientByIdAsync(id);
            if (patient == null) return NotFound();
            return Ok(new { Message = "Patient Retrieved Successfully", Data = patient });
        }
    }
}