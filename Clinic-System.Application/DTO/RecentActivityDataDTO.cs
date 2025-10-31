using Clinic_System.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Clinic_System.Application.DTO
{
    public class RecentActivityDataDTO
    {
       public int newPatientsToday { get; set; }
       public int  appointmentsToday {get; set; }
    }
}
