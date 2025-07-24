namespace UtilityBill.Business.Interfaces
{
    public interface IEmailService
    {
        // Giữ lại phương thức cũ nếu bạn vẫn muốn dùng ở đâu đó
        Task SendPasswordResetEmailAsync(string toEmail, string resetLink);
        Task SendOtpEmailAsync(string toEmail, string otp); // <-- THÊM PHƯƠNG THỨC NÀY
    }
}