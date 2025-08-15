using Clinic_System.Application.Interfaces;
using Clinic_System.Domain.Models;
using Clinic_System.Infrastructure.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Clinic_System.Infrastructure.Services
{
    public class SpecialityService : ISpecialityService
    {
        private readonly SpecialityRepository _repo;

        public SpecialityService(SpecialityRepository repo)
        {
            _repo = repo;
        }

        public async Task<IList<Speciality>> GetAllAsync()
            => await _repo.GetAllAsync();

        public async Task<Speciality?> GetByIdAsync(int id)
            => await _repo.GetByIdAsync(id);

        public async Task<Speciality> CreateAsync(Speciality speciality)
        {
            await _repo.AddAsync(speciality);
            await _repo.SaveChangesAsync();
            return speciality;
        }

        public async Task<bool> UpdateAsync(int id, Speciality speciality)
        {
            var existing = await _repo.GetByIdAsync(id);
            if (existing == null) return false;

            existing.Name = speciality.Name;

            await _repo.UpdateAsync(existing);
            return await _repo.SaveChangesAsync();
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var speciality = await _repo.GetByIdAsync(id);
            if (speciality == null) return false;

            if (await _repo.HasDoctorsAsync(id))
                throw new InvalidOperationException("Cannot delete speciality with assigned doctors");

            await _repo.DeleteAsync(speciality);
            return await _repo.SaveChangesAsync();
        }
    }
}
