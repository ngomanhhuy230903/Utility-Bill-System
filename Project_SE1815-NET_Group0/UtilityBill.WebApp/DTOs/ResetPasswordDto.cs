using System.ComponentModel.DataAnnotations;
namespace UtilityBill.WebApp.DTOs
{
    public class ResetPasswordDto
    {
        // Token và Email sẽ được lấy từ URL và gửi đi trong form ẩn
        public string Token { get; set; }
        public string Email { get; set; }

        [Required(ErrorMessage = "Mật khẩu mới là bắt buộc.")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Mật khẩu và mật khẩu xác nhận không khớp.")]
        public string ConfirmPassword { get; set; }
    }
}