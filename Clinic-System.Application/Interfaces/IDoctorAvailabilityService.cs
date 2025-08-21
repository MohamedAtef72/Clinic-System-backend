using Clinic_System.Application.DTO;

namespace Clinic_System.Application.Interfaces
{
    public interface IDoctorAvailabilityService
    {
        Task AddAvailabilityAsync(DoctorAvailabilityCreateDTO doctorCreateDTO);
        Task<IEnumerable<DoctorAvailabilityDTO>> GetAllAvailabilitiesAsync();

        Task<IEnumerable<DoctorAvailabilityDTO>> GetAvailabilitiesByDoctorAsync(Guid doctorId);
        Task<DoctorAvailabilityDTO?> GetAvailabilityByIdAsync(int id);
        Task UpdateAvailabilityAsync(int id, DateTime startTime, DateTime endTime);
        Task DeleteAvailabilityAsync(int id);
    }
}
