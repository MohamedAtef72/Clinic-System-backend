using Clinic_System.Application.DTO;
using Clinic_System.Application.Interfaces;
using Clinic_System.Domain.Models;
using Clinic_System.Infrastructure.Repositories;

namespace Clinic_System.Application.Services
{
    public class DoctorAvailabilityService : IDoctorAvailabilityService
    {
        private readonly DoctorAvailabilityRepository _availabilityRepository;

        public DoctorAvailabilityService(DoctorAvailabilityRepository availabilityRepository)
        {
            _availabilityRepository = availabilityRepository;
        }

        public async Task AddAvailabilityAsync(DoctorAvailabilityCreateDTO doctorCreateDTO)
        {
            var availability = new DoctorAvailability
            {
                DoctorId = doctorCreateDTO.DoctorId,
                StartTime = doctorCreateDTO.StartTime,
                EndTime = doctorCreateDTO.EndTime,
                IsBooked = false
            };

            await _availabilityRepository.AddAsync(availability);
        }

        public async Task<IEnumerable<DoctorAvailabilityDTO>> GetAvailabilitiesByDoctorAsync(Guid doctorId)
        {
            var availabilities = await _availabilityRepository.GetByDoctorIdAsync(doctorId);
            if (availabilities == null) return null;

            return availabilities.Select(a => new DoctorAvailabilityDTO
            {
                Id = a.Id,
                StartTime = a.StartTime,
                EndTime = a.EndTime,
                IsBooked = a.IsBooked
            });
        }

        public async Task<IEnumerable<DoctorAvailabilityDTO>> GetAllAvailabilitiesAsync()
        {
            var availabilities = await _availabilityRepository.GetAllAsync();
            if (availabilities == null) return null;

            return availabilities.Select(a => new DoctorAvailabilityDTO
            {
                Id = a.Id,
                StartTime = a.StartTime,
                EndTime = a.EndTime,
                IsBooked = a.IsBooked
            });
        }

        public async Task<DoctorAvailabilityDTO?> GetAvailabilityByIdAsync(int id)
        {
            var availability = await _availabilityRepository.GetByIdAsync(id);
            if (availability == null) return null;

            return new DoctorAvailabilityDTO
            {
                Id = availability.Id,
                StartTime = availability.StartTime,
                EndTime = availability.EndTime,
                IsBooked = availability.IsBooked
            };
        }

        public async Task UpdateAvailabilityAsync(int id, DateTime startTime, DateTime endTime)
        {
            var availability = await _availabilityRepository.GetByIdAsync(id);
            if (availability == null) return;

            availability.StartTime = startTime;
            availability.EndTime = endTime;

            await _availabilityRepository.UpdateAsync(availability);
        }

        public async Task DeleteAvailabilityAsync(int id)
        {
            await _availabilityRepository.DeleteAsync(id);
        }
    }
}
