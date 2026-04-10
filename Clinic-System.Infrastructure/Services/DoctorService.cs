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
        private readonly DoctorAvailabilityRepository _availabilityRepo;
        private readonly INotificationService _notificationService;
        private readonly INotificationQueryService _notificationQueryService;
        private readonly Clinic_System.Application.Interfaces.ICacheService _cache;

        public DoctorService(DoctorRepository doctorRepo, DoctorAvailabilityRepository availabilityRepo, INotificationService notificationService, INotificationQueryService notificationQueryService, Clinic_System.Application.Interfaces.ICacheService cache)
        {
            _doctorRepo = doctorRepo;
            _availabilityRepo = availabilityRepo;
            _notificationService = notificationService;
            _notificationQueryService = notificationQueryService;
            _cache = cache;
        }

        public async Task<Doctor> EnsureDoctorExistsOrRestoreAsync(string userId, int specialityId)
        {
            var doctor = await _doctorRepo.GetByUserIdAsync(userId, includeDeleted: true);

            if (doctor != null)
            {
                // Business logic
                doctor.SpecialityId = specialityId;

                // Clean availability via repo
                var unbookedAvailabilities =
                    await _availabilityRepo.GetUnbookedByDoctorIdAsync(doctor.Id);

                if (unbookedAvailabilities.Any())
                {
                    _availabilityRepo.RemoveRange(unbookedAvailabilities);
                }

                await _doctorRepo.SaveChanges();

                return doctor;
            }

            return await AddDoctor(userId, specialityId);
        }

        public async Task<Doctor> AddDoctor(string userId, int specialityId)
        {
            //  Create new doctor
            var newDoctor = new Doctor 
            {
                UserId = userId,
                SpecialityId = specialityId
            };

            await _doctorRepo.AddDoctorAsync(newDoctor);

            //  Notifications (only for NEW doctor)
            var notification = new Notification
            {
                Title = "New Doctor Added",
                Message = $"A new doctor was added (UserId: {userId})",
                IsGlobal = false,
                CreatedAt = DateTime.UtcNow
            };

            await _notificationQueryService.CreateGlobalNotificationAsync(notification);
            await _cache.BumpVersionAsync("doctors:list");
            await _notificationService.SendNotificationToAll(
                notification.Title,
                notification.Message,
                "DoctorAdded");

            return newDoctor;
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

        public async Task<(List<DoctorInfoDTO> Doctors, int TotalCount)> GetAllDoctorsWithDeletedAsync(string? searchName, int pageNumber, int pageSize)
        {
            return await _doctorRepo.GetAllDoctorsWithDeletedAsync(searchName, pageNumber, pageSize);
        }
    }
}
