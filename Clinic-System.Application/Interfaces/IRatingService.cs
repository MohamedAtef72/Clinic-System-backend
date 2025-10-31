using Clinic_System.Application.DTO;
using Clinic_System.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Clinic_System.Application.Interfaces
{
    public interface IRatingService
    {
        Task<RatingReadDTO> GetRateByAppointmentIdAsync(int appointmentId);
        Task<Rating> GetRateByIdAsync(int rateId);
        Task<(Double AverageRate,int TotalRatings)> GetAverageRateByDoctorIdAsync(Guid doctorId);
        Task<bool> AddRateAsync(RatingCreateDTO ratingCreateDTO);
        Task<bool> UpdateRateAsync(int id,RatingUpdateDTO ratingUpdateDTO);
    }
}
