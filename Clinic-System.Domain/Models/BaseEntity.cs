using System;

namespace Clinic_System.Domain.Models
{
    /// <summary>
    /// Base entity with soft delete support.
    /// All entities requiring soft delete should inherit from this.
    /// </summary>
    public abstract class BaseEntity
    {
        public bool IsDeleted { get; set; } = false;
        public DateTime? DeletedAt { get; set; }
    }
}