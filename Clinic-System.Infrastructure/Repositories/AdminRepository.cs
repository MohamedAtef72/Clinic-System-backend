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
            var doctorsCount = await _context.Doctors.CountAsync();

            var patientsCount = await _context.Patients.CountAsync();

            var appointmentCount = await _context.Appointments.CountAsync();

            return (doctorsCount, patientsCount, appointmentCount);
        }

        public async Task<(int newPatientsNumber , int newAppointmentsNumber)> GetRecentData()
        {
            var today = DateTime.UtcNow.Date;
            var tomorrow = today.AddDays(1);

            var newPatientsToday = await _context.Patients
                .CountAsync(p => p.CreatedAt >= today && p.CreatedAt < tomorrow);

            var appointmentsToday = await _context.Appointments
                .CountAsync(a => a.CreatedAt >= today && a.CreatedAt < tomorrow);

            return (newPatientsToday, appointmentsToday);
        }
    }
}
