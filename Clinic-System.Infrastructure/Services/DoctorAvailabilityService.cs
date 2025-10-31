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

        public async Task AddAvailabilityAsync(DoctorAvailabilityCreateDTO dto)
        {
            if (string.IsNullOrEmpty(dto.RecurrencePattern) || dto.RecurrencePattern == "None")
            {
                var singleAvailability = new DoctorAvailability
                {
                    DoctorId = dto.DoctorId,
                    StartTime = dto.StartTime,
                    EndTime = dto.EndTime,
                    IsBooked = false,
                    RecurrencePattern = "None"
                };

                await _availabilityRepository.AddAsync(singleAvailability);
                return;
            }

            if (!dto.RecurrenceEndDate.HasValue)
                throw new ArgumentException("RecurrenceEndDate is required when using a recurrence pattern.");

            var currentStart = dto.StartTime;
            var currentEnd = dto.EndTime;
            var recurrenceId = Guid.NewGuid(); 

            while (currentStart <= dto.RecurrenceEndDate.Value)
            {
                var availability = new DoctorAvailability
                {
                    DoctorId = dto.DoctorId,
                    StartTime = currentStart,
                    EndTime = currentEnd,
                    IsBooked = false,
                    RecurrencePattern = dto.RecurrencePattern,
                    RecurrenceEndDate = dto.RecurrenceEndDate,
                    SeriesId = recurrenceId
                };

                await _availabilityRepository.AddAsync(availability);

                currentStart = dto.RecurrencePattern switch
                {
                    "Weekly" => currentStart.AddDays(7),
                    "BiWeekly" => currentStart.AddDays(14),
                    _ => currentStart.AddDays(7) 
                };
                currentEnd = currentStart.Add(dto.EndTime - dto.StartTime);
            }
        }

        public async Task<IEnumerable<DoctorAvailabilityDTO>> GetAvailabilitiesByDoctorAsync(Guid doctorId)
        {
            var availabilities = await _availabilityRepository.GetByDoctorIdAsync(doctorId);
            return availabilities.Select(a => new DoctorAvailabilityDTO
            {
                Id = a.Id,
                StartTime = a.StartTime,
                EndTime = a.EndTime,
                IsBooked = a.IsBooked,
            });
        }

        public async Task<IEnumerable<DoctorAvailabilityDTO>> GetAllAvailabilitiesAsync()
        {
            var availabilities = await _availabilityRepository.GetAllAsync();
            return availabilities.Select(a => new DoctorAvailabilityDTO
            {
                Id = a.Id,
                StartTime = a.StartTime,
                EndTime = a.EndTime,
                IsBooked = a.IsBooked,
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
                IsBooked = availability.IsBooked,
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
