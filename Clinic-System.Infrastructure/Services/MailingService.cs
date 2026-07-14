using Clinic_System.Application.Interfaces;
using Clinic_System.Domain.Constant;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;
using Microsoft.AspNetCore.Http;


namespace Clinic_System.Infrastructure.Services
{
    public class MailingService : IMailingServices
    {
        private readonly MailSettings _mailSettings;
        private readonly IAppointmentService _appointmentService;

        public MailingService(IOptions<MailSettings> mailSettings,IAppointmentService appointmentService)
        {
            _mailSettings = mailSettings.Value;
            _appointmentService = appointmentService;
        }

        public async Task SendEmailAsync(string mailTo, string subject, string body, IList<IFormFile> attachments = null)
        {
            var email = new MimeMessage();

            email.Sender = MailboxAddress.Parse(_mailSettings.Mail);
            email.From.Add(new MailboxAddress(_mailSettings.DisplayName, _mailSettings.Mail));
            email.To.Add(MailboxAddress.Parse(mailTo));
            email.Subject = subject;

            var builder = new BodyBuilder { HtmlBody = body };

            if (attachments != null && attachments.Count > 0)
            {
                foreach (var file in attachments)
                {
                    using var ms = new MemoryStream();
                    await file.CopyToAsync(ms);
                    builder.Attachments.Add(file.FileName, ms.ToArray(), ContentType.Parse(file.ContentType));
                }
            }

            email.Body = builder.ToMessageBody();

            using var smtp = new SmtpClient();
            smtp.ServerCertificateValidationCallback = (s, c, h, e) => true;
            await smtp.ConnectAsync(_mailSettings.Host, _mailSettings.Port,SecureSocketOptions.StartTls);
            await smtp.AuthenticateAsync(_mailSettings.Mail, _mailSettings.Password);
            await smtp.SendAsync(email);
            await smtp.DisconnectAsync(true);
        }

        public async Task SendReminderEmailAsync()
        {
            var appointments = await _appointmentService.GetAppointmentsWithRemindersAsync();

            foreach (var appointment in appointments)
            {
                var body = $@"
                <h2>Appointment Reminder</h2>
                <p>Dear {appointment.Patient.User.UserName},</p>
                <p>You have an appointment on {appointment.Date:dd/MM/yyyy HH:mm}</p>
                <p>Regards,<br/>MedClinic Team</p>";

                await SendEmailAsync(
                    appointment.Patient.User.Email,
                    "Appointment Reminder",
                    body);

                appointment.ReminderSent = true;
            }

            await _appointmentService.SaveChangesAsync();
        }

    }
}