using ECO.BLL.DTO.Auth;
using Microsoft.Extensions.Configuration;
using MimeKit;
using MailKit.Security;
using ECO.DAL.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.DataProtection;


//using MimeKit;
using System;
using System.Collections.Generic;
using System.Text;

namespace ECO.BLL.Services.Email
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration configuration;
        private readonly AppDbContext db;
        private readonly IDataProtector protector;

        public EmailService(IConfiguration configuration, AppDbContext db, IDataProtectionProvider dataProtection)
        {
            this.configuration = configuration;
            this.db = db;
            protector = dataProtection.CreateProtector("ECO.Settings.SmtpPassword");
        }
        public async Task SendEmail(EmailDto emailDto)
        {
            MimeMessage message = new MimeMessage();
            var settings = await db.StoreSettings.AsNoTracking()
                .Where(x => x.SmtpHost != null && x.SmtpUsername != null)
                .OrderByDescending(x => x.SmtpFrom == emailDto.From)
                .ThenByDescending(x => x.Id)
                .FirstOrDefaultAsync();
            var from = settings?.SmtpFrom ?? settings?.SmtpUsername ?? configuration["EmailSetting:From"];
            var host = settings?.SmtpHost ?? configuration["EmailSetting:SMTP"];
            var port = settings?.SmtpPort ?? configuration.GetValue<int?>("EmailSetting:Port");
            var username = settings?.SmtpUsername ?? configuration["EmailSetting:UserName"];
            string? password = null;
            if (settings?.SmtpPasswordProtected is { Length: > 0 })
            {
                try
                {
                    password = protector.Unprotect(settings.SmtpPasswordProtected);
                }
                catch (System.Security.Cryptography.CryptographicException)
                {
                    password = configuration["EmailSetting:Password"];
                }
            }
            else
            {
                password = configuration["EmailSetting:Password"];
            }
            if (string.IsNullOrWhiteSpace(from) || string.IsNullOrWhiteSpace(host) ||
                !port.HasValue || string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
                throw new InvalidOperationException("إعدادات SMTP غير مكتملة أو يلزم إعادة حفظ كلمة مرور SMTP في إعدادات المتجر.");
            message.From.Add(new MailboxAddress(
                string.IsNullOrWhiteSpace(settings?.EmailBrandName) ? "Eco" : settings!.EmailBrandName!, from));
            message.Subject=emailDto.Subject;
            message.To.Add(new MailboxAddress(emailDto.To, emailDto.To));
            message.Body = new TextPart(MimeKit.Text.TextFormat.Html)
            {
                Text=emailDto.Content
            };
            using (var smtp = new MailKit.Net.Smtp.SmtpClient())
            {
                using var cancellation = new CancellationTokenSource(TimeSpan.FromSeconds(10));
                try
                {
                    smtp.Timeout = 10000;
                    Console.WriteLine("Connecting to the configured SMTP server to send account email.");
                    await smtp.ConnectAsync(
                        host,
                        port.Value,
                        settings?.SmtpUseSsl == false ? SecureSocketOptions.StartTls : SecureSocketOptions.SslOnConnect,
                        cancellation.Token);
                    Console.WriteLine("SMTP connection established; authenticating sender account.");
                    await smtp.AuthenticateAsync(
                        username,
                        password,
                        cancellation.Token);
                    await smtp.SendAsync(message, cancellation.Token);
                    Console.WriteLine("Account email sent successfully.");
                }
                catch (OperationCanceledException) when (cancellation.IsCancellationRequested)
                {
                    Console.WriteLine("SMTP did not respond within 10 seconds.");
                    throw new TimeoutException("The email service did not respond within 10 seconds.");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Unable to send account email through SMTP: {ex.GetType().Name}");
                    throw;
                }
                finally
                {
                    if (smtp.IsConnected)
                        await smtp.DisconnectAsync(true);
                } 
            };
        }
    }
}
