using Clinic_System.Application.DTO;
using Clinic_System.Domain.Models;
using Clinic_System.Infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

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

        // Get Receptionist From DB
        public async Task<Receptionist> GetReceptionistByIdAsync(string userId)
        {
            return await _db.Receptionists.FirstOrDefaultAsync(e => e.UserId == userId);
        }

        // Update Receptionist Async
        public async Task<IdentityResult> UpdateReceptionistAsync(string userId, UserEditProfile receptionEdit)
        {
            var receptionistFromDB = await GetReceptionistByIdAsync(userId);
            if (receptionistFromDB == null)
            {
                return IdentityResult.Failed(new IdentityError { Description = "Receptionist not found." });
            }

            receptionistFromDB.ShiftStart = (TimeSpan)receptionEdit.ShiftStart;
            receptionistFromDB.ShiftEnd = (TimeSpan)receptionEdit.ShiftEnd;

            _db.Receptionists.Update(receptionistFromDB);
            var changes = await _db.SaveChangesAsync();

            if (changes > 0)
                return IdentityResult.Success;

            return IdentityResult.Failed(new IdentityError { Description = "No changes were made." });
        }
    }
}
