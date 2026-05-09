using Clinic_System.Application.Interfaces;
using Clinic_System.Application.DTO;
using Clinic_System.Domain.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace Clinic_System.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class PatientController : ControllerBase
    {
        private readonly IPatientService _patientService;
        private readonly ICacheService _cache;

        public PatientController(IPatientService patientService, ICacheService cache)
        {
            _patientService = patientService;
            _cache = cache;
        }

        [HttpGet("GetAll")]
        [EnableRateLimiting("ReadPolicy")]
        public async Task<IActionResult> GetAll(string? searchName, string? gender, int pageNumber = 1, int pageSize = 5)
        {
            var isAdmin = User.IsInRole("Admin");
            List<Clinic_System.Application.DTO.PatientInfoDTO> patients;
            int totalCount;

            if (isAdmin)
            {
                // Admin: show all patients (deleted and not deleted)
                var (pList, tCount) = await _patientService.GetAllPatientsWithDeletedAsync(searchName, gender, pageNumber, pageSize);
                patients = pList;
                totalCount = tCount;
            }
            else
            {
                var version = await _cache.GetVersionAsync("patients:list");
                var sanitizedSearch = string.IsNullOrEmpty(searchName) ? "" : searchName;
                var cacheKey = $"patients:list:{version}:{sanitizedSearch}:{pageNumber}:{pageSize}";
                var cached = await _cache.GetAsync<PatientsListDTO>(cacheKey);

                if (cached != null)
                {
                    patients = cached.Patients;
                    totalCount = cached.TotalCount;
                }
                else
                {
                    var (pList, tCount) = await _patientService.GetAllPatientsAsync(searchName, gender, pageNumber, pageSize);
                    patients = pList;
                    totalCount = tCount;
                    var cacheEntry = new PatientsListDTO
                    {
                        Patients = patients,
                        TotalCount = totalCount
                    };
                    await _cache.SetAsync(cacheKey, cacheEntry, TimeSpan.FromMinutes(10));
                }
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
        [EnableRateLimiting("ReadPolicy")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var version = await _cache.GetVersionAsync($"patient:{id}");
            var cacheKey = $"patient:{id}:{version}";
            var cached = await _cache.GetAsync<PatientInfoDTO>(cacheKey);

            PatientInfoDTO patient = null;

            if (cached != null)
            {
                patient = cached;
            }
            else
            {
                patient = await _patientService.GetPatientByIdAsync(id);
                if (patient == null) return NotFound();
                await _cache.SetAsync(cacheKey, patient, TimeSpan.FromMinutes(10));
            }

            return Ok(new { Message = "Patient Retrieved Successfully", Data = patient });
        }
    }
}