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
    public class VisitRepository
    {
        private readonly AppDbContext _db;

        public VisitRepository(AppDbContext context)
        {
            _db = context;
        }

        public async Task<IEnumerable<Visit>> GetAllAsync()
        {
            return await _db.Visits
                .Include(v => v.Appointment)
                .ToListAsync();
        }

        public async Task<Visit> GetByIdAsync(int id)
        {
            return await _db.Visits
                .Include(v => v.Appointment)
                .FirstOrDefaultAsync(v => v.Id == id);
        }

        public async Task<IEnumerable<Visit>> GetVisitsByDoctorIdAsync(Guid doctorId)
        {
            return await _db.Visits
                .Include(v => v.Appointment)
                .Where(v => v.Appointment.Availability.DoctorId == doctorId)
                .ToListAsync();
        }

        public async Task<IEnumerable<Visit>> GetVisitsByPatientIdAsync(Guid patientId)
        {
            return await _db.Visits
                .Include(v => v.Appointment)
                .Where(v => v.Appointment.PatientId == patientId)
                .ToListAsync();
        }

        public async Task AddAsync(Visit visit)
        {
            await _db.Visits.AddAsync(visit);
            await _db.SaveChangesAsync();
        }

        public async Task UpdateAsync(Visit visit)
        {
            _db.Visits.Update(visit);
            await _db.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var visit = await _db.Visits.FindAsync(id);
            if (visit != null)
            {
                _db.Visits.Remove(visit);
                await _db.SaveChangesAsync();
            }
        }
    }
}
