using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Clinic_System.Domain.Models
{
    public class Speciality
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }
        [JsonIgnore]
        public ICollection<Doctor>? Doctors { get; set; }
    }
}
