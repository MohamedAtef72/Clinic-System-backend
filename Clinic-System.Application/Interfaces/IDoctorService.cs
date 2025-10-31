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
        Task AddDoctor(Doctor newDoctor);
        Task<(List<DoctorInfoDTO> Doctors, int TotalCount)> GetAllDoctorsAsync(string? searchName,int pageNumber, int pageSize);
        Task<DoctorInfoDTO> GetDoctorByIdAsync(Guid id);
        Task<DoctorInfoDTO> GetDoctorByUserIdAsync(string userId);
        Task<IdentityResult> UpdateDoctorAsync(string userId, UserEditProfile doctorEdit);
        Task<bool> UpdateDoctorPriceAsync(Guid doctorId, int price);
    }
}
