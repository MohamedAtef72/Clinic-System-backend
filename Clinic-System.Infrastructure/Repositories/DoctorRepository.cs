using Clinic_System.Domain.Models;
using Clinic_System.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Clinic_System.Infrastructure.Repositories
{
    public class DoctorRepository
    {
        private readonly AppDbContext _db;

        public DoctorRepository(AppDbContext db)
        {
            _db = db;
        }

        public async Task AddDoctor(Doctor newDoctor)
        {
            if (newDoctor != null)
            {
                await _db.Doctors.AddAsync(newDoctor);
                await _db.SaveChangesAsync();
            }
        }
    }

}