using Clinic_System.Application.DTO;
using Clinic_System.Domain.Models;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Clinic_System.Application.Interfaces
{
    public interface IReceptionistRepository
    {
        Task AddReceptionist(Receptionist newReceptionist);
        Task<Receptionist> GetReceptionistByIdAsync(Guid id);
        Task<Receptionist> GetReceptionistByUserIdAsync(string userId);
        Task<IdentityResult> UpdateReceptionistAsync(string userId, UserEditProfile receptionEdit);
    }
}
