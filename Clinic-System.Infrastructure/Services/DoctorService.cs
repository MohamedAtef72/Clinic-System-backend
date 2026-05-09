using Clinic_System.Application.DTO;
using Clinic_System.Application.Interfaces;
using Clinic_System.Domain.Models;
using Clinic_System.Infrastructure.Repositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Clinic_System.Infrastructure.Services
{
    public class DoctorService : IDoctorService
    {
        private readonly IDoctorRepository _doctorRepo;
        private readonly IDoctorAvailabilityService _availabilityService;
        private readonly INotificationService _notificationService;
        private readonly INotificationQueryService _notificationQueryService;
        private readonly ICacheService _cache;
        private readonly ILogger<DoctorService> _logger;
        private readonly UserManager<ApplicationUser> _userManager;

        public DoctorService(UserManager<ApplicationUser> userManager, IDoctorRepository doctorRepo, IDoctorAvailabilityService availabilityService, INotificationService notificationService, INotificationQueryService notificationQueryService, ICacheService cache, ILogger<DoctorService> logger)
        {
            _doctorRepo = doctorRepo;
            _availabilityService = availabilityService;
            _notificationService = notificationService;
            _notificationQueryService = notificationQueryService;
            _cache = cache;
            _logger = logger;
            _userManager = userManager;
        }

        public async Task<Doctor> EnsureDoctorExistsOrRestoreAsync(string userId, int specialityId)
        {
            var doctor = await _doctorRepo.GetByUserIdAsync(
                userId,
                includeDeleted: true);

            if (doctor != null)
            {
                doctor.SpecialityId = specialityId;

                await _availabilityService.DeleteUnbookedByDoctorIdAsync(doctor.Id);

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

            // Track: Add Doctor to Repository
            await _doctorRepo.AddDoctorAsync(newDoctor);

            // Track: Get Doctor Details
            var userName = await _userManager.Users
                .Where(u => u.Id == userId)
                .Select(u => u.UserName)
                .FirstOrDefaultAsync();

            //  Notifications (only for NEW doctor)
            var notification = new Notification
            {
                Title = "New Doctor Added",
                Message = $"A new doctor was added (UserName: {userName})",
                IsGlobal = false,
                CreatedAt = DateTime.UtcNow
            };

            var notificationTask =
                _notificationQueryService.CreateGlobalNotificationAsync(notification);

            var cacheTask =
                _cache.BumpVersionAsync("doctors:list");

            var signalRTask =
                _notificationService.SendNotificationToAll(
                    notification.Title,
                    notification.Message,
                    "DoctorAdded");

            // choose max time for all methods and wait it
            await Task.WhenAll(notificationTask, cacheTask, signalRTask);

            return newDoctor;
        }

        public async Task<(List<DoctorInfoDTO> Doctors, int TotalCount)> GetAllDoctorsAsync(string? searchName, string? gender, int? speciality, int pageNumber, int pageSize)
        {
            return await _doctorRepo.GetAllDoctorsAsync(searchName, gender, speciality, pageNumber, pageSize);
        }

        public async Task<DoctorInfoDTO> GetDoctorByIdAsync(Guid id)
        {
            return await _doctorRepo.GetDoctorByIdAsync(id);
        }
        public async Task<DoctorInfoDTO> GetDoctorByUserIdAsync(string userId)
        {
            return await _doctorRepo.GetDoctorByUserIdAsync(userId);
        }
        public async Task<bool> UpdateDoctorPriceAsync(Guid doctorId, int price)
        {
            return await _doctorRepo.UpdateDoctorPriceAsync(doctorId, price);
        }

        public async Task<(List<DoctorInfoDTO> Doctors, int TotalCount)> GetAllDoctorsWithDeletedAsync(string? searchName, string? gender, int? speciality, int pageNumber, int pageSize)
        {
            return await _doctorRepo.GetAllDoctorsWithDeletedAsync(searchName, gender, speciality, pageNumber, pageSize);
        }
    }
}
