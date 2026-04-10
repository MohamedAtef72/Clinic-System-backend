using AutoMapper;
using Clinic_System.Application.DTO;
using Clinic_System.Application.Interfaces;
using Clinic_System.Domain.Models;
using Clinic_System.Infrastructure.Data;
using Clinic_System.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Org.BouncyCastle.Crypto;
using System.Reflection.Metadata.Ecma335;

namespace Clinic_System.Infrastructure.Services
{
    public class AppointmentService : IAppointmentService
    {
        private readonly AppointmentRepository _appointmentRepository;
        private readonly DoctorAvailabilityRepository _doctorAvailabilityRepository;
        private readonly IMapper _mapper;
        private readonly INotificationQueryService _notificationQueryService;
        private readonly INotificationService _notificationService;
        private readonly DoctorRepository _doctorRepository;
        private readonly AppDbContext _db;


        public AppointmentService(AppointmentRepository appointmentRepository , IMapper mapper, DoctorAvailabilityRepository doctorAvailabilityRepository, DoctorRepository doctorRepository, INotificationService notificationService, INotificationQueryService notificationQueryService, AppDbContext db)
        {
            _appointmentRepository = appointmentRepository;
            _mapper = mapper;
            _doctorAvailabilityRepository = doctorAvailabilityRepository;
            _notificationQueryService = notificationQueryService;
            _notificationService = notificationService;
            _doctorRepository = doctorRepository;
            _db = db;
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

            // Check if availability is active
            if (!availability.IsActive)
                throw new InvalidOperationException("This availability slot is no longer active.");

            // Check if Doctor is deactivated
            if (availability.Doctor.User.IsDeleted)
                throw new InvalidOperationException("The doctor for this slot is no longer active.");

            // Check if Slot is booked
            if (availability.IsBooked)
                throw new InvalidOperationException("This slot is already booked.");

            // Check if Patient is deactivated
            var patient = await _db.Patients
                .Include(p => p.User)
                .FirstOrDefaultAsync(p => p.Id == dto.PatientId);
            if (patient == null || patient.User.IsDeleted)
                throw new InvalidOperationException("Patient is no longer active or does not exist.");

            // Check For Old Appointment with same availability
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

                // Create notification for the doctor (persist notification and a user-notification)
                var doctorUserId = await _doctorRepository.GetUserIdByDoctorIdAsync(availability.DoctorId);
                var doctorId = doctorUserId ?? availability.DoctorId.ToString();
                var notification = new Clinic_System.Domain.Models.Notification
                {
                    Title = "AppointmentBooked",
                    Message = $"You have a new appointment on {dto.Date:yyyy-MM-dd HH:mm}",
                    IsGlobal = false,
                    CreatedAt = DateTime.UtcNow
                };

                await _notificationQueryService.CreateNotificationForUserAsync(doctorId, notification);

                // Send real-time notification to the doctor user
                await _notificationService.SendNotificationToUser(doctorId, notification.Title, notification.Message, "AppointmentBooked");
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

            // Notify doctor about status change
            var doctorUserId = await _doctorRepository.GetUserIdByDoctorIdAsync(appointment.Availability.DoctorId);
            var doctorId = doctorUserId ?? appointment.Availability.DoctorId.ToString();
            var notification2 = new Clinic_System.Domain.Models.Notification
            {
                Title = "Appointment Status Updated",
                Message = $"Appointment {appointment.Date} status changed to {appointment.AppointmentStatus}.",
                IsGlobal = false,
                CreatedAt = DateTime.UtcNow
            };
            await _notificationQueryService.CreateNotificationForUserAsync(doctorId, notification2);
            await _notificationService.SendNotificationToUser(doctorId, notification2.Title, notification2.Message, "AppointmentStatusChanged");
        }


        public async Task DeleteAppointmentAsync(int id)
        {
            var appointment = await _appointmentRepository.GetByIdAsync(id);
            if (appointment == null)
                throw new ArgumentException("Appointment not found.");

            // Soft delete appointment
            appointment.IsDeleted = true;
            appointment.DeletedAt = DateTime.UtcNow;
            await _appointmentRepository.UpdateAsync(appointment);
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
