using Clinic_System.Application.DTO;
using Clinic_System.Application.Interfaces;
using Clinic_System.Domain.Models;
using Clinic_System.Infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Clinic_System.Infrastructure.Repositories
{
    public class ReceptionistRepository : IReceptionistRepository
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
        public async Task<Receptionist> GetReceptionistByIdAsync(Guid id)
        {
            return await _db.Receptionists.Include(d => d.User)
                .FirstOrDefaultAsync(e => e.Id == id);
        }
        public async Task<Receptionist> GetReceptionistByUserIdAsync(string userId)
        {
            return await _db.Receptionists.Include(d => d.User)
                .FirstOrDefaultAsync(e => e.UserId == userId);
        }

        // Update Receptionist Async
        public async Task<IdentityResult> UpdateReceptionistAsync(string userId, UserEditProfile receptionEdit)
        {
            var receptionistFromDB = await GetReceptionistByUserIdAsync(userId);

            if (receptionistFromDB == null)
            {
                return IdentityResult.Failed(new IdentityError { Description = "Receptionist not found." });
            }

            bool isUpdated = false;

            if (receptionEdit.ShiftStart != null && receptionistFromDB.ShiftStart != receptionEdit.ShiftStart)
            {
                receptionistFromDB.ShiftStart = receptionEdit.ShiftStart.Value;
                isUpdated = true;
            }

            if (receptionEdit.ShiftEnd != null && receptionistFromDB.ShiftEnd != receptionEdit.ShiftEnd)
            {
                receptionistFromDB.ShiftEnd = receptionEdit.ShiftEnd.Value;
                isUpdated = true;
            }

            if (!isUpdated)
            {
                return IdentityResult.Failed(new IdentityError { Description = "No changes detected." });
            }

            await _db.SaveChangesAsync(); 

            return IdentityResult.Success;
        }
    }
}
