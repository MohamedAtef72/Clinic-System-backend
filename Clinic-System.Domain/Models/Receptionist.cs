using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Clinic_System.Domain.Models
{
    public class Receptionist
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        [Column(TypeName = "time")]

        public TimeSpan ShiftStart { get; set; }

        [Required]
        [Column(TypeName = "time")]

        public TimeSpan ShiftEnd { get; set; }

        [Required]
        [ForeignKey(nameof(User))]
        public string UserId { get; set; }

        // Navigation
        public ApplicationUser User { get; set; }
    }
}
