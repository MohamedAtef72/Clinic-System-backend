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
        public async Task<IActionResult> GetAll(string? searchName, int pageNumber = 1, int pageSize = 5)
        {
            var isAdmin = User.IsInRole("Admin");
            List<Clinic_System.Application.DTO.PatientInfoDTO> patients;
            int totalCount;

            if (isAdmin)
            {
                // Admin: show all patients (deleted and not deleted)
                var (pList, tCount) = await _patientService.GetAllPatientsWithDeletedAsync(searchName, pageNumber, pageSize);
                patients = pList;
                totalCount = tCount;
            }
            else
            {
                var (pList, tCount) = await _patientService.GetAllPatientsAsync(searchName, pageNumber, pageSize);
                patients = pList;
                totalCount = tCount;
            }

            if (patients == null || !patients.Any())
            {
                return NotFound(new { Message = "Patients Not Found" });
            }
            return Ok(new {Message = "Patients Retrieved Successfully",
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize,
                Data = patients});
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var patient = await _patientService.GetPatientByIdAsync(id);
            if (patient == null) return NotFound();
            return Ok(new { Message = "Patient Retrieved Successfully", Data = patient });
        }
    }
}