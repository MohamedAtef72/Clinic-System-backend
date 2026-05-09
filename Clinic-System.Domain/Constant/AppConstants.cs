namespace Clinic_System.Domain.Constant
{
    /// <summary>
    /// Centralized constants for the application to avoid magic strings
    /// </summary>
    public static class AppConstants
    {
        /// <summary>
        /// User role constants
        /// </summary>
        public static class Roles
        {
            public const string Admin = "Admin";
            public const string Doctor = "Doctor";
            public const string Patient = "Patient";
            public const string Receptionist = "Receptionist";
        }

        /// <summary>
        /// Rate limiting policy names
        /// </summary>
        public static class RateLimitPolicies
        {
            public const string Auth = "AuthPolicy";
            public const string Read = "ReadPolicy";
            public const string Write = "WritePolicy";
        }

        /// <summary>
        /// Cache key prefixes for Redis
        /// </summary>
        public static class CacheKeys
        {
            public const string DoctorsList = "doctors:list";
            public const string UserProfile = "user:profile";
            public const string PatientsList = "patients:list";
            public const string AppointmentsList = "appointments:list";
            public const string SpecialitiesList = "specialities:list";

            /// <summary>
            /// Gets version key for a given prefix to enable cache invalidation
            /// </summary>
            public static string GetVersionKey(string prefix) => $"{prefix}:version";
        }

        /// <summary>
        /// Appointment status constants
        /// </summary>
        public static class AppointmentStatuses
        {
            public const string Scheduled = "Scheduled";
            public const string Completed = "Completed";
            public const string Cancelled = "Cancelled";
            public const string NoShow = "NoShow";
        }

        /// <summary>
        /// Notification types for SignalR and database
        /// </summary>
        public static class NotificationTypes
        {
            public const string DoctorAdded = "DoctorAdded";
            public const string AppointmentBooked = "AppointmentBooked";
            public const string AppointmentCancelled = "AppointmentCancelled";
            public const string AppointmentCompleted = "AppointmentCompleted";
            public const string NewDoctor = "New Doctor Added";
        }

        /// <summary>
        /// Cloudinary settings
        /// </summary>
        public static class Cloudinary
        {
            public const string DefaultFolder = "clinic_app_images";
            public const int ImageQuality = 75;
            public const int MaxImageSize = 5242880; // 5MB in bytes
        }

        /// <summary>
        /// Email-related constants
        /// </summary>
        public static class Email
        {
            public const string RegistrationSubject = "Clinic-System | Your Account Credentials";
            public const string PasswordResetSubject = "Clinic-System | Password Reset Request";
        }
    }
}
