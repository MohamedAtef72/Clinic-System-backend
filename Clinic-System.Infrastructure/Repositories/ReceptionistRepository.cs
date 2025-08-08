using Clinic_System.Domain.Models;
using Clinic_System.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Clinic_System.Infrastructure.Repositories
{
    public class ReceptionistRepository
    {
        private readonly AppDbContext _db;

        public ReceptionistRepository(AppDbContext db)
        {
            _db = db;
        }

        public async Task AddReceptionist(Receptionist newReceptionist)
        {
            if (newReceptionist != null)
            {
                await _db.Receptionists.AddAsync(newReceptionist);
                await _db.SaveChangesAsync();
            }
        }
    }
}
