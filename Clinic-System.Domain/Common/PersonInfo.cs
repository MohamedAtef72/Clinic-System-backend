using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Clinic_System.Domain.Common
{
    public abstract class PersonInfo
    {
        [Required]
        public string Country { get; set; }

        [Required]
        public string Gender { get; set; }

        [Required]
        public DateTime DateOfBirth { get; set; }

        [Required]
        public DateTime RegisterDate { get; set; }
    }
}

