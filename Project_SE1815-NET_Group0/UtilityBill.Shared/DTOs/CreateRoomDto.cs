using System.ComponentModel.DataAnnotations;

namespace UtilityBill.Shared.DTOs
{
    public class CreateRoomDto
    {
        [Required(ErrorMessage = "Số phòng là bắt buộc.")]
        [StringLength(50, ErrorMessage = "Số phòng không được vượt quá 50 ký tự.")]
        public string RoomNumber { get; set; }

        [StringLength(50, ErrorMessage = "Tên block/dãy không được vượt quá 50 ký tự.")]
        public string? Block { get; set; }

        public int? Floor { get; set; }

        [Required(ErrorMessage = "Diện tích là bắt buộc.")]
        [Range(1, 1000, ErrorMessage = "Diện tích phải trong khoảng từ 1 đến 1000 m2.")]
        public decimal Area { get; set; }

        [Required(ErrorMessage = "Giá phòng là bắt buộc.")]
        [Range(1, 100000000, ErrorMessage = "Giá phòng không hợp lệ.")]
        public decimal Price { get; set; }
    }
}