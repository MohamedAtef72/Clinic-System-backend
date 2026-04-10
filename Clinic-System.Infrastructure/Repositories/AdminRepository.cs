using Clinic_System.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Clinic_System.Infrastructure.Repositories
{
    public class AdminRepository
    {
        private readonly AppDbContext _context;

        public AdminRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<(int doctorsCount , int patientsCount, int appointmentCount)> GetInfo()
        {
            // Count all doctors
            var doctorsCount = await _context.Doctors
                .CountAsync();

            // Count all patients 
            var patientsCount = await _context.Patients
                .CountAsync();

            // Count appointments (auto-filtered by global query filter for IsDeleted)
            var appointmentCount = await _context.Appointments.CountAsync();

            return (doctorsCount, patientsCount, appointmentCount);
        }

        public async Task<(int newPatientsNumber , int newAppointmentsNumber)> GetRecentData()
        {
            var today = DateTime.UtcNow.Date;
            var tomorrow = today.AddDays(1);

            // Count new patients created today (excluding deactivated users)
            var newPatientsToday = await _context.Patients
                .Where(p => !p.User.IsDeleted && p.CreatedAt >= today && p.CreatedAt < tomorrow)
                .CountAsync();

            // Count appointments created today (auto-filtered by global query filter for IsDeleted)
            var appointmentsToday = await _context.Appointments
                .CountAsync(a => a.CreatedAt >= today && a.CreatedAt < tomorrow);

            return (newPatientsToday, appointmentsToday);
        }
    }
}
