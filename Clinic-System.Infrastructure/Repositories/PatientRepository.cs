using Clinic_System.Domain.Models;
using Clinic_System.Infrastructure.Data;

namespace Clinic_System.Infrastructure.Repositories
{
    public class PatientRepository
    {
        private readonly AppDbContext _db;

        public PatientRepository(AppDbContext db)
        {
            _db = db;
        }

        public async Task AddPatient(Patient newPatient)
        {
            if (newPatient != null)
            {
                await _db.Patients.AddAsync(newPatient);
                await _db.SaveChangesAsync();
            }
        }
    }
}
