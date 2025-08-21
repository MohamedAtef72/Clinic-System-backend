using Clinic_System.Domain.Models;
using Clinic_System.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Clinic_System.Infrastructure.Repositories
{
    public class DoctorAvailabilityRepository 
    {
        private readonly AppDbContext _db;

        public DoctorAvailabilityRepository(AppDbContext db)
        {
            _db = db;
        }

        public async Task AddAsync(DoctorAvailability availability)
        {
            await _db.DoctorAvailabilities.AddAsync(availability);
            await _db.SaveChangesAsync();
        }

        public async Task<IEnumerable<DoctorAvailability>> GetAllAsync()
        {
            return await _db.DoctorAvailabilities
                                 .Include(d => d.Doctor)
                                 .ToListAsync();
        }

        public async Task<IEnumerable<DoctorAvailability>> GetByDoctorIdAsync(Guid doctorId)
        {
            return await _db.DoctorAvailabilities
                .Where(a => a.DoctorId == doctorId)
                .ToListAsync();
        }

        public async Task<DoctorAvailability?> GetByIdAsync(int id)
        {
            return await _db.DoctorAvailabilities.FindAsync(id);
        }

        public async Task UpdateAsync(DoctorAvailability availability)
        {
            _db.DoctorAvailabilities.Update(availability);
            await _db.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var availability = await GetByIdAsync(id);
            if (availability != null)
            {
                _db.DoctorAvailabilities.Remove(availability);
                await _db.SaveChangesAsync();
            }
        }
    }
}
