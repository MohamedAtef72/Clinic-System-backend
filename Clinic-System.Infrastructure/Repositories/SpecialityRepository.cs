using Clinic_System.Domain.Models;
using Clinic_System.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Clinic_System.Infrastructure.Repositories
{
    public class SpecialityRepository
    {
        private readonly AppDbContext _db;

        public SpecialityRepository(AppDbContext db)
        {
            _db = db;
        }

        public async Task<IList<Speciality>> GetAllAsync()
            => await _db.Specialities.ToListAsync();

        public async Task<Speciality?> GetByIdAsync(int id)
            => await _db.Specialities.FindAsync(id);

        public async Task AddAsync(Speciality speciality)
            => await _db.Specialities.AddAsync(speciality);

        public Task UpdateAsync(Speciality speciality)
        {
            _db.Specialities.Update(speciality);
            return Task.CompletedTask;
        }

        public Task DeleteAsync(Speciality speciality)
        {
            _db.Specialities.Remove(speciality);
            return Task.CompletedTask;
        }

        public async Task<bool> HasDoctorsAsync(int specialityId)
            => await _db.Doctors.AnyAsync(d => d.SpecialityId == specialityId);

        public async Task<bool> SaveChangesAsync()
            => await _db.SaveChangesAsync() > 0;
    }
}
