// File: UtilityBill.WebApp/DTOs/UserDto.cs

namespace UtilityBill.WebApp.DTOs
{
    public class UserDto
    {
        public string Id { get; set; } // <-- THÊM THUỘC TÍNH ID CÒN THIẾU

        public string UserName { get; set; }
        public string Email { get; set; }
        public string FullName { get; set; }

        // Thuộc tính Token này chỉ có giá trị khi đăng nhập,
        // khi lấy danh sách user nó có thể là null, điều này hoàn toàn bình thường.
        public string? Token { get; set; }
    }
}   