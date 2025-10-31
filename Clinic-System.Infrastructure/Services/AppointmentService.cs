using AutoMapper;
using Clinic_System.Application.DTO;
using Clinic_System.Application.Interfaces;
using Clinic_System.Domain.Models;
using Clinic_System.Infrastructure.Repositories;
using Org.BouncyCastle.Crypto;
using System.Reflection.Metadata.Ecma335;

namespace Clinic_System.Infrastructure.Services
{
    public class AppointmentService : IAppointmentService
    {
        private readonly AppointmentRepository _appointmentRepository;
        private readonly DoctorAvailabilityRepository _doctorAvailabilityRepository;
        private readonly IMapper _mapper;


        public AppointmentService(AppointmentRepository appointmentRepository , IMapper mapper, DoctorAvailabilityRepository doctorAvailabilityRepository)
        {
            _appointmentRepository = appointmentRepository;
            _mapper = mapper;
            _doctorAvailabilityRepository = doctorAvailabilityRepository;
        }

        public async Task<(List<AppointmentDTO> Appointments, int TotalCount)> GetAllAppointmentsAsync( string? status, int pageNumber, int pageSize)
        {
            return await _appointmentRepository.GetAllAppointmentsAsync( status,pageNumber , pageSize);
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
            // Check For Availability
            var availability = await _doctorAvailabilityRepository.GetByIdAsync(dto.AvailabilityId);
            if (availability == null)
                throw new ArgumentException("Invalid availability ID.");

            // Chec For Is Not Booked
            if (availability.IsBooked)
                throw new InvalidOperationException("This slot is already booked.");

            // Check For Have Old Appointment with same id
            var existingAppointment = await _appointmentRepository
                .GetByAvailabilityIdAsync(dto.AvailabilityId);

            if (existingAppointment != null && existingAppointment.AppointmentStatus != "Cancelled")
                throw new InvalidOperationException("This slot already has an appointment.");

            // Begin Transaction
            using var transaction = await _appointmentRepository.BeginTransactionAsync();

            try
            {
                // Create Appointment
                var appointment = new Appointment
                {
                    AvailabilityId = dto.AvailabilityId,
                    PatientId = dto.PatientId,
                    Date = dto.Date,
                    AppointmentStatus = dto.AppointmentStatus
                };

                await _appointmentRepository.AddAsync(appointment);

                // Update Availability
                availability.IsBooked = true;
                await _doctorAvailabilityRepository.UpdateAsync(availability);

                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task UpdateAppointmentStatusAsync(UpdateAppointmentDTO dto)
        {
            var appointment = await _appointmentRepository.GetByIdAsync(dto.Id);
            if (appointment == null)
                throw new ArgumentException("Appointment not found.");

            // Update the appointment status
            appointment.AppointmentStatus = dto.AppointmentStatus;

            if (dto.AppointmentStatus == "Cancelled")
            {
                var availability = await _doctorAvailabilityRepository.GetByIdAsync(appointment.AvailabilityId);
                if (availability != null)
                {
                    availability.IsBooked = false;
                    await _doctorAvailabilityRepository.UpdateAsync(availability);
                }
            }

            await _appointmentRepository.UpdateAsync(appointment);
        }


        public async Task DeleteAppointmentAsync(int id)
        {
            await _appointmentRepository.DeleteAsync(id);
        }
        public async Task<(List<AppointmentDTO> Appointments, int totalCount)>
            GetAppointmentsByDoctorIdAsync(string? status, Guid doctorId, int pageNumber, int pageSize, DateTime? startDate, DateTime? endDate)
        {
            return await _appointmentRepository.GetAppointmentsByDoctorIdAsync(status,doctorId, pageNumber, pageSize, startDate, endDate);
        }


        public async Task<(List<AppointmentDTO> Appointments, int totalCount)> GetAppointmentsByPatientIdAsync(string? status, Guid patientId, int pageNumber, int pageSize)
        {
            return await _appointmentRepository.GetAppointmentsByPatientIdAsync(status,patientId , pageNumber , pageSize);
        }
    }
}
