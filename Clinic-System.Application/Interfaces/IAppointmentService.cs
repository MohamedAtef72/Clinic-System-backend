using Clinic_System.Application.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Clinic_System.Application.Interfaces
{
    public interface IAppointmentService
    {
        Task<(List<AppointmentDTO> Appointments , int TotalCount)> GetAllAppointmentsAsync( string? status,int pageNumber , int pageSize);
        Task<(List<AppointmentDTO> Appointments , int totalCount)> GetAppointmentsByDoctorIdAsync(string? status, Guid doctorId ,int pageNumber ,int pageSize, DateTime?startDate , DateTime? endDate);
        Task<(List<AppointmentDTO> Appointments, int totalCount)> GetAppointmentsByPatientIdAsync(string? status,Guid patientId, int pageNumber , int pageSize);
        Task<AppointmentDTO> GetAppointmentByIdAsync(int id);
        Task CreateAppointmentAsync(CreateAppointmentDTO dto);
        Task UpdateAppointmentStatusAsync(UpdateAppointmentDTO dto);
        Task DeleteAppointmentAsync(int id);
    }
}
