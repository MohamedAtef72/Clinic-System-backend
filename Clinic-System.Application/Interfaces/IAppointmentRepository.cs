using Clinic_System.Application.DTO;
using Clinic_System.Domain.Models;
using Microsoft.EntityFrameworkCore.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Clinic_System.Application.Interfaces
{
    public interface IAppointmentRepository
    {
        Task<(List<AppointmentDTO> Appointments, int totalCount)> GetAllAppointmentsAsync(string? status, int pageNumber, int pageSize);
        Task<Appointment> GetByIdAsync(int id);
        Task AddAsync(Appointment appointment);
        Task UpdateAsync(Appointment appointment);
        Task DeleteAsync(int id);
        Task<(List<AppointmentDTO> Appointments, int totalCount)> GetAppointmentsByDoctorIdAsync(string? status, Guid doctorId, int pageNumber, int pageSize, DateTime? startDate, DateTime? endDate);
        Task<(List<AppointmentDTO> Appointments, int totalCount)> GetAppointmentsByPatientIdAsync(string? status, Guid patientId, int pageNumber, int pageSize);
        Task<Appointment?> GetByAvailabilityIdAsync(int availabilityId);
        Task<IDbContextTransaction> BeginTransactionAsync();
    }
}
