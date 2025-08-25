using AutoMapper;
using Clinic_System.Application.DTO;
using Clinic_System.Application.Interfaces;
using Clinic_System.Domain.Models;
using Clinic_System.Infrastructure.Repositories;
using Org.BouncyCastle.Crypto;

namespace Clinic_System.Infrastructure.Services
{
    public class AppointmentService : IAppointmentService
    {
        private readonly AppointmentRepository _appointmentRepository;
        private readonly IMapper _mapper;


        public AppointmentService(AppointmentRepository appointmentRepository , IMapper mapper)
        {
            _appointmentRepository = appointmentRepository;
            _mapper = mapper;
        }

        public async Task<IEnumerable<AppointmentDTO>> GetAllAppointmentsAsync()
        {
            var appointments = await _appointmentRepository.GetAllAppointmentsAsync();

            return appointments.Select(a => new AppointmentDTO
            {
                Id = a.Id,
                PatientId = a.PatientId,
                AvailabilityId = a.AvailabilityId,
                Date = a.Date,
                AppointmentStatus = a.AppointmentStatus
            }).ToList();
        }

        public async Task<AppointmentDTO> GetAppointmentByIdAsync(int id)
        {
            var appointment = await _appointmentRepository.GetByIdAsync(id);
            if (appointment == null) return null;

            return new AppointmentDTO
            {
                Id = appointment.Id,
                AvailabilityId = appointment.AvailabilityId,
                PatientId = appointment.PatientId,
                Date = appointment.Date,
                AppointmentStatus = appointment.AppointmentStatus
            };
        }

        public async Task CreateAppointmentAsync(CreateAppointmentDTO dto)
        {
            var appointment = new Appointment
            {
                AvailabilityId = dto.AvailabilityId,
                PatientId = dto.PatientId,
                Date = dto.Date,
                AppointmentStatus = "Scheduled"
            };

            await _appointmentRepository.AddAsync(appointment);
        }

        public async Task UpdateAppointmentStatusAsync(UpdateAppointmentDTO dto)
        {
            var appointment = await _appointmentRepository.GetByIdAsync(dto.Id);
            if (appointment != null)
            {
                appointment.AppointmentStatus = dto.AppointmentStatus;
                await _appointmentRepository.UpdateAsync(appointment);
            }
        }

        public async Task DeleteAppointmentAsync(int id)
        {
            await _appointmentRepository.DeleteAsync(id);
        }
        public async Task<IEnumerable<AppointmentDTO>> GetAppointmentsByDoctorIdAsync(Guid doctorId)
        {
            var appointments = await _appointmentRepository.GetAppointmentsByDoctorIdAsync(doctorId);
            return _mapper.Map<IEnumerable<AppointmentDTO>>(appointments);
        }

        public async Task<IEnumerable<AppointmentDTO>> GetAppointmentsByPatientIdAsync(Guid patientId)
        {
            var appointments = await _appointmentRepository.GetAppointmentsByPatientIdAsync(patientId);
            return _mapper.Map<IEnumerable<AppointmentDTO>>(appointments);
        }
    }
}
