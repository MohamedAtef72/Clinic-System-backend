using Clinic_System.Application.DTO;
using Clinic_System.Application.Interfaces;
using Clinic_System.Domain.Models;
using Clinic_System.Infrastructure.Repositories;
using Microsoft.AspNetCore.Identity;

namespace Clinic_System.Infrastructure.Services
{
    public class DoctorService : IDoctorService
    {
        private readonly DoctorRepository _doctorRepo;

        public DoctorService(DoctorRepository doctorRepo)
        {
            _doctorRepo = doctorRepo;
        }

        public async Task AddDoctor(Doctor newDoctor)
        {
            await _doctorRepo.AddDoctorAsync(newDoctor);
        }

        public async Task<(List<DoctorInfoDTO> Doctors, int TotalCount)> GetAllDoctorsAsync(string? searchName, int pageNumber, int pageSize)
        {
            return await _doctorRepo.GetAllDoctorsAsync(searchName,pageNumber, pageSize);
        }

        public async Task<DoctorInfoDTO> GetDoctorByIdAsync(Guid id)
        {
            return await _doctorRepo.GetDoctorByIdAsync(id);
        }
        public async Task<DoctorInfoDTO> GetDoctorByUserIdAsync(string userId)
        {
            return await _doctorRepo.GetDoctorByUserIdAsync(userId);
        }

        public async Task<IdentityResult> UpdateDoctorAsync(string userId, UserEditProfile doctorEdit)
        {
            return await _doctorRepo.UpdateDoctorAsync(userId, doctorEdit);
        }
        public async Task<bool> UpdateDoctorPriceAsync(Guid doctorId, int price)
        {
            return await _doctorRepo.UpdateDoctorPriceAsync(doctorId, price);
        }
    }
}
