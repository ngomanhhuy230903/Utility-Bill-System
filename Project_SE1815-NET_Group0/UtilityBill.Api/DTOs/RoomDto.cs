// File: UtilityBill.WebApp/DTOs/RoomDto.cs
namespace UtilityBill.Api.DTOs
{
    // DTO này dùng để nhận dữ liệu phòng từ API
    public class RoomDto
    {
        public int Id { get; set; }
        public string RoomNumber { get; set; }
        public string? Block { get; set; }
        public int? Floor { get; set; }
        public decimal Area { get; set; }
        public decimal Price { get; set; }
        public string Status { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}