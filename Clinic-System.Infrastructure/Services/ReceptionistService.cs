using Clinic_System.Application.DTO;
using Clinic_System.Application.Interfaces;
using Clinic_System.Domain.Models;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Clinic_System.Infrastructure.Services
{
    public class ReceptionistService : IReceptionistService
    {
        private readonly IReceptionistRepository _receptionistRepository;

        public ReceptionistService(IReceptionistRepository receptionistRepository)
        {
            _receptionistRepository = receptionistRepository;
        }

        // Add new receptionist
        public async Task AddReceptionist(Receptionist newReceptionist)
        {
            await _receptionistRepository.AddReceptionist(newReceptionist);
        }

        // Get receptionist by ID
        public async Task<Receptionist> GetReceptionistByIdAsync(Guid id)
        {
            return await _receptionistRepository.GetReceptionistByIdAsync(id);
        }

        // Get receptionist by user ID
        public async Task<Receptionist> GetReceptionistByUserIdAsync(string userId)
        {
            return await _receptionistRepository.GetReceptionistByUserIdAsync(userId);
        }

        // Update receptionist information
        public async Task<IdentityResult> UpdateReceptionistAsync(string userId, UserEditProfile receptionEdit)
        {
            return await _receptionistRepository.UpdateReceptionistAsync(userId, receptionEdit);
        }
    }
}
