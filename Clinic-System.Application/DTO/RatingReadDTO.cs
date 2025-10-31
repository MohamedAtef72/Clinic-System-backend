using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Clinic_System.Application.DTO
{
    public class RatingReadDTO
    {
        public int Id { get; set; }
        public int Rate { get; set; }
        public string? Comment { get; set; }
    }
}
