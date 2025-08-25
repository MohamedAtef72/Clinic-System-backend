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
        Task<IEnumerable<AppointmentDTO>> GetAllAppointmentsAsync();
        Task<IEnumerable<AppointmentDTO>> GetAppointmentsByDoctorIdAsync(Guid doctorId);
        Task<IEnumerable<AppointmentDTO>> GetAppointmentsByPatientIdAsync(Guid patientId);
        Task<AppointmentDTO> GetAppointmentByIdAsync(int id);
        Task CreateAppointmentAsync(CreateAppointmentDTO dto);
        Task UpdateAppointmentStatusAsync(UpdateAppointmentDTO dto);
        Task DeleteAppointmentAsync(int id);
    }
}
