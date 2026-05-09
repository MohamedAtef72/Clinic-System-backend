using Clinic_System.Application.DTO;
using Clinic_System.Domain.Models;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Clinic_System.Application.Interfaces
{
    public interface IDoctorService
    {
        Task<Doctor> AddDoctor(string userId, int specialityId);
        Task<(List<DoctorInfoDTO> Doctors, int TotalCount)> GetAllDoctorsAsync(string? searchName,string? gender, int? speciality, int pageNumber, int pageSize);
        Task<DoctorInfoDTO> GetDoctorByIdAsync(Guid id);
        Task<DoctorInfoDTO> GetDoctorByUserIdAsync(string userId);
        Task<bool> UpdateDoctorPriceAsync(Guid doctorId, int price);
        Task<(List<DoctorInfoDTO> Doctors, int TotalCount)> GetAllDoctorsWithDeletedAsync(string? searchName, string? gender, int? speciality, int pageNumber, int pageSize);
        Task<Doctor> EnsureDoctorExistsOrRestoreAsync(string userId, int specialityId);
    }
}
