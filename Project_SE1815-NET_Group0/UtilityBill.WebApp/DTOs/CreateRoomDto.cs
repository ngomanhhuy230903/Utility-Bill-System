using System.ComponentModel.DataAnnotations;
namespace UtilityBill.WebApp.DTOs
{
    public class CreateRoomDto
    {
        [Required]
        public string RoomNumber { get; set; }
        public string? Block { get; set; }
        public int? Floor { get; set; }
        [Required]
        public decimal Area { get; set; }
        [Required]
        public decimal Price { get; set; }
    }
}