using System.ComponentModel.DataAnnotations;

namespace UtilityBill.WebApp.DTOs
{
    public class RegisterDto
    {
        [Required(ErrorMessage = "Tên đăng nhập là bắt buộc.")]
        // SỬA LẠI ERROR MESSAGE CHO ĐÚNG CHUẨN
        [StringLength(100, MinimumLength = 3, ErrorMessage = "{0} phải dài từ {2} đến {1} ký tự.")]
        [Display(Name = "Tên đăng nhập")] // Thêm DisplayName để {0} hiển thị đẹp hơn
        public string UserName { get; set; }

        [Required(ErrorMessage = "Họ và tên là bắt buộc.")]
        [Display(Name = "Họ và tên")]
        public string FullName { get; set; }

        [Required(ErrorMessage = "Email là bắt buộc.")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ.")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Số điện thoại là bắt buộc.")] // <-- Thêm dòng này
        [RegularExpression(@"^(0\d{9})$", ErrorMessage = "Số điện thoại phải có 10 chữ số và bắt đầu bằng số 0.")]
        [Display(Name = "Số điện thoại")]
        public string PhoneNumber { get; set; } // <-- Bỏ dấu ?

        [Required(ErrorMessage = "Mật khẩu là bắt buộc.")]
        [DataType(DataType.Password)]
        // SỬA LẠI ERROR MESSAGE CHO ĐÚNG CHUẨN
        [StringLength(100, MinimumLength = 6, ErrorMessage = "{0} phải dài từ {2} đến {1} ký tự.")]
        [Display(Name = "Mật khẩu")]
        public string Password { get; set; }
    }
}