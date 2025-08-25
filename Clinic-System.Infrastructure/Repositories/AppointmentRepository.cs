using Clinic_System.Domain.Models;
using Clinic_System.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Clinic_System.Infrastructure.Repositories
{
    public class AppointmentRepository
    {
        private readonly AppDbContext _db;

        public AppointmentRepository(AppDbContext db)
        {
            _db = db;
        }

        public async Task<IEnumerable<Appointment>> GetAllAppointmentsAsync()
        {
            return await _db.Appointments
                .Include(a => a.Patient)
                .Include(a => a.Availability)
                .ToListAsync();
        }

        public async Task<Appointment> GetByIdAsync(int id)
        {
            return await _db.Appointments
                .Include(a => a.Patient)
                .Include(a => a.Availability)
                .FirstOrDefaultAsync(a => a.Id == id);
        }

        public async Task AddAsync(Appointment appointment)
        {
            await _db.Appointments.AddAsync(appointment);
            await _db.SaveChangesAsync();
        }

        public async Task UpdateAsync(Appointment appointment)
        {
            _db.Appointments.Update(appointment);
            await _db.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var appointment = await _db.Appointments.FindAsync(id);
            if (appointment != null)
            {
                _db.Appointments.Remove(appointment);
                await _db.SaveChangesAsync();
            }
        }

        public async Task<IEnumerable<Appointment>> GetAppointmentsByDoctorIdAsync(Guid doctorId)
        {
            return await _db.Appointments
                .Include(a => a.Patient)
                .Include(a => a.Availability)
                .ThenInclude(av => av.Doctor)
                .Where(a => a.Availability.DoctorId == doctorId)
                .ToListAsync();
        }

        public async Task<IEnumerable<Appointment>> GetAppointmentsByPatientIdAsync(Guid patientId)
        {
            return await _db.Appointments
                .Include(a => a.Patient)
                .Include(a => a.Availability)
                .ThenInclude(av => av.Doctor)
                .Where(a => a.PatientId == patientId)
                .ToListAsync();
        }
    }
}
