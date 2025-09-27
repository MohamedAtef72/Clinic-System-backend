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
        Task<(List<DoctorInfoDTO> Doctors, int TotalCount)> GetAllDoctorsAsync(int pageNumber, int pageSize);
        Task<DoctorInfoDTO> GetDoctorByIdAsync(string userId);
        Task<IdentityResult> UpdateDoctorAsync(string userId, UserEditProfile doctorEdit);
    }
}
