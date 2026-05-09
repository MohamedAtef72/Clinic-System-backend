using Clinic_System.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Clinic_System.Application.Interfaces
{
    public interface IRatingRepository
    {
        Task<Rating> GetByAppointmentIdAsync(int appointmentId);
        Task<Rating> GetByIdAsync(int rateId);
        Task<List<Rating>> GetRatingsByDoctorIdAsync(Guid doctorId);
        Task AddRateAsync(Rating model);
        Task UpdateRateAsync(Rating model);
    }
}
