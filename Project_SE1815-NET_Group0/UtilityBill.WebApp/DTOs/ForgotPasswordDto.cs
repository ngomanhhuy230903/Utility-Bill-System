using System.ComponentModel.DataAnnotations;
namespace UtilityBill.WebApp.DTOs
{
    public class ForgotPasswordDto
    {
        [Required(ErrorMessage = "Vui lòng nhập email của bạn.")]
        [EmailAddress(ErrorMessage = "Địa chỉ email không hợp lệ.")]
        public string Email { get; set; }
    }
}