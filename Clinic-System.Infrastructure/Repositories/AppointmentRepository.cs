using Clinic_System.Application.DTO;
using Clinic_System.Domain.Models;
using Clinic_System.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace Clinic_System.Infrastructure.Repositories
{
    public class AppointmentRepository
    {
        private readonly AppDbContext _db;

        public AppointmentRepository(AppDbContext db)
        {
            _db = db;
        }

        public async Task<(List<AppointmentDTO> Appointments, int totalCount)> GetAllAppointmentsAsync(string? status, int pageNumber , int pageSize)
        {
            var query =  _db.Appointments
                .Include(a => a.Visit)
                .Include(a => a.Availability)
                .ThenInclude(av => av.Doctor).AsQueryable();

            if (!string.IsNullOrEmpty(status))
            {
                query = query.Where(a => a.AppointmentStatus == status);
            }

            var totalCount = await query.CountAsync();

            var appointments = await
                query.Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(appointment => new AppointmentDTO
                {
                    Id = appointment.Id,
                    VisitId = appointment.Visit.Id,
                    PatientId = appointment.PatientId,
                    DoctorId = appointment.Availability.DoctorId,
                    AvailabilityId = appointment.AvailabilityId,
                    AppointmentStatus = appointment.AppointmentStatus,
                }
                    ).ToListAsync();
            return (appointments , totalCount);
        }

        public async Task<Appointment> GetByIdAsync(int id)
        {
            return await _db.Appointments
                .Include(a => a.Patient)
                .Include(a => a.Availability)
                .Include(a => a.Visit)
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

        public async Task<(List<AppointmentDTO> Appointments, int totalCount)>
            GetAppointmentsByDoctorIdAsync(string? status, Guid doctorId, int pageNumber, int pageSize, DateTime? startDate, DateTime? endDate)
        {
            var query = _db.Appointments
                .Include(a => a.Patient)
                .Include(a => a.Visit)
                .Include(a => a.Availability)
                .ThenInclude(av => av.Doctor)
                .Where(a => a.Availability.DoctorId == doctorId)
                .AsQueryable();

            if (!String.IsNullOrEmpty(status))
            {
                query = query.Where(a => a.AppointmentStatus == status);
            }

            if (startDate.HasValue)
                query = query.Where(a => a.Date >= startDate.Value);

            if (endDate.HasValue)
                query = query.Where(a => a.Date <= endDate.Value);

            var totalCount = await query.CountAsync();

            var appointments = await query
                .OrderBy(a => a.Date)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(appointment => new AppointmentDTO
                {
                    Id = appointment.Id,
                    VisitId = appointment.Visit.Id,
                    PatientId = appointment.PatientId,
                    DoctorId = appointment.Availability.DoctorId,
                    AvailabilityId = appointment.AvailabilityId,
                    Date = appointment.Date,
                    AppointmentStatus = appointment.AppointmentStatus,
                    MedicalHistory = appointment.Patient.MedicalHistory
                })
                .ToListAsync();

            return (appointments, totalCount);
        }

        public async Task<(List<AppointmentDTO> Appointments, int totalCount)> GetAppointmentsByPatientIdAsync(string? status, Guid patientId, int pageNumber, int pageSize)
        {
            var query = _db.Appointments
               .Include(a => a.Patient)
               .Include(a => a.Visit)
               .Include(a => a.Availability)
               .ThenInclude(av => av.Doctor)
               .Where(a => a.PatientId == patientId)
               .AsQueryable();

            if (!String.IsNullOrEmpty(status))
            {
                query = query.Where(a => a.AppointmentStatus == status);
            }

            var totalCount = await query.CountAsync();

            var appointments = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(
                    appointment => new AppointmentDTO
                    {
                        Id= appointment.Id,
                        PatientId = appointment.PatientId,
                        VisitId = appointment.Visit.Id,
                        DoctorId= appointment.Availability.DoctorId,
                        AvailabilityId= appointment.AvailabilityId,
                        Date = appointment.Date,
                        AppointmentStatus = appointment.AppointmentStatus,
                    }
                ).ToListAsync();

            return (appointments, totalCount);
        }

        public async Task<Appointment?> GetByAvailabilityIdAsync(int availabilityId)
        {
            return await _db.Appointments
                .FirstOrDefaultAsync(a => a.AvailabilityId == availabilityId);
        }

        public async Task<IDbContextTransaction> BeginTransactionAsync()
        {
            return await _db.Database.BeginTransactionAsync();
        }

    }
}
