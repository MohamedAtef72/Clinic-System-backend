using Clinic_System.Domain.Models;
using Clinic_System.Domain.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Reflection.Emit;

namespace Clinic_System.Infrastructure.Data
{
    public class AppDbContext : IdentityDbContext<ApplicationUser>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        // DbSets
        public DbSet<Patient> Patients { get; set; }
        public DbSet<Doctor> Doctors { get; set; }
        public DbSet<Receptionist> Receptionists { get; set; }
        public DbSet<Appointment> Appointments { get; set; }
        public DbSet<Visit> Visits { get; set; }
        public DbSet<Speciality> Specialities { get; set; }
        public DbSet<DoctorAvailability> DoctorAvailabilities { get; set; }
        public DbSet<RefreshToken> RefreshTokens { get; set; }
        public DbSet<Rating> Rating { get; set; }
        public DbSet<Clinic_System.Domain.Models.Notification> Notifications { get; set; }
        public DbSet<Clinic_System.Domain.Models.UserNotification> UserNotifications { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<Speciality>()
                .HasIndex(s => s.Name)
                .IsUnique();

            // Relationships

            builder.Entity<Patient>()
                .HasOne(p => p.User)
                .WithOne(u => u.Patient)
                .HasForeignKey<Patient>(p => p.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Doctor>()
                .HasOne(d => d.User)
                .WithOne(u => u.Doctor)
                .HasForeignKey<Doctor>(d => d.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Doctor>()
                .HasOne(d => d.Speciality)
                .WithMany(s => s.Doctors)
                .HasForeignKey(d => d.SpecialityId);

            builder.Entity<Receptionist>()
                .HasOne(r => r.User)
                .WithOne(u => u.Receptionist)
                .HasForeignKey<Receptionist>(r => r.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Appointment>()
                .HasOne(a => a.Patient)
                .WithMany(p => p.Appointments)
                .HasForeignKey(a => a.PatientId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<DoctorAvailability>()
                .HasOne(da => da.Doctor)
                .WithMany(d => d.Availabilities)
                .HasForeignKey(da => da.DoctorId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Visit>()
                .HasOne(v => v.Appointment)
                .WithOne(a => a.Visit)
                .HasForeignKey<Visit>(v => v.AppointmentId);

            builder.Entity<Clinic_System.Domain.Models.Notification>(b =>
            {
                b.HasKey(n => n.Id);
                b.Property(n => n.Title).IsRequired();
                b.Property(n => n.Message).IsRequired();
                b.Property(n => n.IsGlobal).HasDefaultValue(false);
                b.Property(n => n.CreatedAt).HasColumnType("datetime2");
            });
            builder.Entity<Notification>()
                .HasIndex(un => un.CreatedAt);

            builder.Entity<UserNotification>(b =>
            {
                b.HasKey(un => un.Id);

                b.HasOne(un => un.Notification)
                 .WithMany(n => n.UserNotifications) 
                 .HasForeignKey(un => un.NotificationId)
                 .OnDelete(DeleteBehavior.Cascade);

                b.Property(un => un.UserId).IsRequired();
                b.Property(un => un.IsRead).HasDefaultValue(false);
            });
            builder.Entity<UserNotification>()
                 .HasIndex(un => un.UserId);

            builder.Entity<UserNotification>()
                .HasIndex(un => un.NotificationId);
        }
    }

}
