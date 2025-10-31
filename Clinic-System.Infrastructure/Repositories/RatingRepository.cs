using Clinic_System.Domain.Models;
using Clinic_System.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Clinic_System.Infrastructure.Repositories
{
    public class RatingRepository
    {
        private readonly AppDbContext _context;

        public RatingRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Rating> GetByAppointmentIdAsync(int appointmentId)
        {
            return await _context.Rating.FirstOrDefaultAsync(a => a.AppointmentId == appointmentId);
        }

        public async Task<Rating> GetByIdAsync(int rateId)
        {
            return await _context.Rating.FirstOrDefaultAsync(a => a.Id == rateId);
        }

        public async Task<List<Rating>> GetRatingsByDoctorIdAsync(Guid doctorId)
        {
            return await _context.Rating
                .Where(r => r.DoctorId == doctorId)
                .ToListAsync();
        }

        public async Task AddRateAsync(Rating model)
        {
            await _context.Rating.AddAsync(model);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateRateAsync(Rating model)
        {
            _context.Rating.Update(model);
            await _context.SaveChangesAsync();
        }
    }
}
