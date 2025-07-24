// File: Api/DTOs/ResetPasswordWithOtpDto.cs
namespace UtilityBill.Api.DTOs
{
    public class ResetPasswordWithOtpDto
    {
        public string Email { get; set; }
        public string Otp { get; set; }
        public string NewPassword { get; set; }
    }
}