using Microsoft.AspNetCore.Http;

namespace Clinic_System.Application.Interfaces
{   
    public interface IMailingServices
    {
        Task SendEmailAsync(string mailTo, string subject, string body, IList<IFormFile> attachments = null);
    }
}