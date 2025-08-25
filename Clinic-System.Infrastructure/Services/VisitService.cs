using AutoMapper;
using Clinic_System.Application.DTO;
using Clinic_System.Application.Interfaces;
using Clinic_System.Domain.Models;
using Clinic_System.Infrastructure.Repositories;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;

namespace Clinic_System.Infrastructure.Services
{
    public class VisitService : IVisitService
    {
        private readonly VisitRepository _visitRepository;
        private readonly IMapper _mapper;

        public VisitService(VisitRepository visitRepository, IMapper mapper)
        {
            _visitRepository = visitRepository;
            _mapper = mapper;
        }

        public async Task<IEnumerable<VisitReadDTO>> GetAllVisitsAsync()
        {
            var visits = await _visitRepository.GetAllAsync();
            return _mapper.Map<IEnumerable<VisitReadDTO>>(visits);
        }

        public async Task<VisitReadDTO> GetVisitByIdAsync(int id)
        {
            var visit = await _visitRepository.GetByIdAsync(id);
            return _mapper.Map<VisitReadDTO>(visit);
        }

        public async Task<IEnumerable<VisitReadDTO>> GetVisitsByDoctorIdAsync(Guid doctorId)
        {
            var visits =  await _visitRepository.GetVisitsByDoctorIdAsync(doctorId);
            return _mapper.Map<IEnumerable<VisitReadDTO>>(visits);
        }

        public async Task<IEnumerable<VisitReadDTO>> GetVisitsByPatientIdAsync(Guid patientId)
        {
            var visits = await _visitRepository.GetVisitsByPatientIdAsync(patientId);
            return _mapper.Map<IEnumerable<VisitReadDTO>>(visits);

        }

        public async Task AddVisitAsync(VisitCreateDTO visitDto)
        {
            var visit = _mapper.Map<Visit>(visitDto);
            await _visitRepository.AddAsync(visit);
        }

        public async Task UpdateVisitAsync(VisitReadDTO visitDto)
        {
            var visit = _mapper.Map<Visit>(visitDto);
            await _visitRepository.UpdateAsync(visit);
        }

        public async Task DeleteVisitAsync(int id)
        {
            await _visitRepository.DeleteAsync(id);
        }
    }
}
