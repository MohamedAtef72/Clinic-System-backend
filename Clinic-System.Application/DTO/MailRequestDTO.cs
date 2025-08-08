using Microsoft.AspNetCore.Http;

namespace Clinic_System.Application.DTO
{
    public class MailRequestDTO
    {
        public string ToEmail { get; set; } = string.Empty;
        public string Subject { get; set; } = string.Empty;
        public string Body { get; set; } = string.Empty;
        public List<IFormFile>? Attachments { get; set; }
    }
}