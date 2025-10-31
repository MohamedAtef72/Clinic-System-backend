using Clinic_System.Application.DTO;
using Clinic_System.Application.Interfaces;
using Clinic_System.Domain.Models;
using Clinic_System.Infrastructure.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Clinic_System.Infrastructure.Services
{
    public class RatingService : IRatingService
    {
        private readonly RatingRepository _rateRepository;

        public RatingService(RatingRepository rateRepository)
        {
            _rateRepository = rateRepository;
        }

        public async Task<bool> AddRateAsync(RatingCreateDTO ratingCreateDTO)
        {
            if (ratingCreateDTO == null)
                throw new ArgumentNullException(nameof(ratingCreateDTO), "Rating data cannot be null.");

            if (ratingCreateDTO.Rate < 1 || ratingCreateDTO.Rate > 5)
                throw new ArgumentException("Rate value must be between 1 and 5.");

            var existingRate = await _rateRepository.GetByAppointmentIdAsync(ratingCreateDTO.AppointmentId);
            if (existingRate != null)
                throw new ArgumentException("A rating already exists for this appointment.");

            var rate = new Rating
            {
                AppointmentId = ratingCreateDTO.AppointmentId,
                DoctorId = ratingCreateDTO.DoctorId,
                PatientId = ratingCreateDTO.PatientId,
                Rate = ratingCreateDTO.Rate,
                Comment = ratingCreateDTO.Comment?.Trim(),
            };

            await _rateRepository.AddRateAsync(rate);
            return true;
        }

        public async Task<(double AverageRate, int TotalRatings)> GetAverageRateByDoctorIdAsync(Guid doctorId)
        {
            if (doctorId == Guid.Empty)
                throw new ArgumentException("Invalid doctor ID.");

            var ratings = await _rateRepository.GetRatingsByDoctorIdAsync(doctorId);

            if (ratings == null || !ratings.Any())
                return (0, 0);

            double averageRate = Math.Round(ratings.Average(r => r.Rate), 2);
            int totalRatings = ratings.Count;

            return (averageRate, totalRatings);
        }

        public async Task<RatingReadDTO> GetRateByAppointmentIdAsync(int appointmentId)
        {
            if (appointmentId <= 0)
                throw new ArgumentException("Invalid appointment ID.");

            var rate = await _rateRepository.GetByAppointmentIdAsync(appointmentId);
            if (rate == null)
                return null;

            return new RatingReadDTO
            {
                Id = rate.Id,
                Rate = rate.Rate,
                Comment = rate.Comment,
            };
        }

        public async Task<Rating> GetRateByIdAsync(int rateId)
        {
            if (rateId <= 0)
                throw new ArgumentException("Invalid rating ID.");

            var rate = await _rateRepository.GetByIdAsync(rateId);
            return rate ?? throw new KeyNotFoundException($"Rating with ID {rateId} not found.");
        }

        public async Task<bool> UpdateRateAsync(int id, RatingUpdateDTO ratingUpdateDTO)
        {
            if (ratingUpdateDTO == null)
                throw new ArgumentNullException(nameof(ratingUpdateDTO), "Update model cannot be null.");

            if (ratingUpdateDTO.Rate < 1 || ratingUpdateDTO.Rate > 5)
                throw new ArgumentException("Rate value must be between 1 and 5.");

            var rateFromDb = await _rateRepository.GetByIdAsync(id);
            if (rateFromDb == null)
                throw new KeyNotFoundException($"No rating found with ID {id}.");

            rateFromDb.Rate = ratingUpdateDTO.Rate;
            rateFromDb.Comment = ratingUpdateDTO.Comment?.Trim();

            await _rateRepository.UpdateRateAsync(rateFromDb);
            return true;
        }
    }
}
