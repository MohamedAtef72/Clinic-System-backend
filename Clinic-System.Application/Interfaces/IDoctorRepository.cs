using Clinic_System.Application.DTO;
using Clinic_System.Domain.Models;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Clinic_System.Application.Interfaces
{
    public interface IDoctorRepository
    {
        Task AddDoctorAsync(Doctor newDoctor);
        Task<(List<DoctorInfoDTO> Doctors, int TotalCount)> GetAllDoctorsAsync(string? searchName, string? gender, int? speciality, int pageNumber, int pageSize);
        Task<(List<DoctorInfoDTO> Doctors, int TotalCount)> GetAllDoctorsWithDeletedAsync(string? searchName, string? gender, int? speciality, int pageNumber, int pageSize);
        Task<DoctorInfoDTO?> GetDoctorByIdAsync(Guid id);
        Task<DoctorInfoDTO?> GetDoctorByUserIdAsync(string userId);
        Task<bool> UpdateDoctorPriceAsync(Guid doctorId, int price);
        Task<string?> GetUserIdByDoctorIdAsync(Guid doctorId);
        Task<Doctor?> GetByUserIdAsync(string userId, bool includeDeleted = false);
        Task SaveChanges();
    }
}
