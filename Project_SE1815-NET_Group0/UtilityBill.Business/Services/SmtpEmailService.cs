using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;
using UtilityBill.Business.Interfaces;
using UtilityBill.Business.Settings;

namespace UtilityBill.Business.Services
{
    public class SmtpEmailService : IEmailService
    {
        private readonly SmtpSettings _smtpSettings;

        public SmtpEmailService(IOptions<SmtpSettings> smtpSettings)
        {
            _smtpSettings = smtpSettings.Value;
        }

        public async Task SendPasswordResetEmailAsync(string toEmail, string resetLink)
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(_smtpSettings.SenderName, _smtpSettings.SenderEmail));
            message.To.Add(new MailboxAddress(toEmail, toEmail));
            message.Subject = "Yêu cầu Reset Mật khẩu";

            var bodyBuilder = new BodyBuilder();
            bodyBuilder.HtmlBody = $"""
                <h2>Yêu cầu Reset Mật khẩu</h2>
                <p>Chúng tôi đã nhận được yêu cầu reset mật khẩu cho tài khoản của bạn.</p>
                <p>Vui lòng nhấn vào đường link dưới đây để đặt lại mật khẩu mới. Link này chỉ có hiệu lực trong một thời gian ngắn.</p>
                <p><a href="{resetLink}" style="padding: 10px 15px; background-color: #0d6efd; color: white; text-decoration: none; border-radius: 5px;">Reset Mật khẩu</a></p>
                <p>Nếu bạn không yêu cầu điều này, vui lòng bỏ qua email này.</p>
                <p>Trân trọng,<br/>Ban quản lý Khu trọ An Bình</p>
                """;
            message.Body = bodyBuilder.ToMessageBody();

            using var client = new SmtpClient();
            await client.ConnectAsync(_smtpSettings.Server, _smtpSettings.Port, SecureSocketOptions.StartTls);
            await client.AuthenticateAsync(_smtpSettings.Username, _smtpSettings.Password);
            await client.SendAsync(message);
            await client.DisconnectAsync(true);
        }
    }
}