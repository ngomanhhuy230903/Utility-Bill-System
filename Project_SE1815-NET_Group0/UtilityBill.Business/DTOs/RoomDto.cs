using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UtilityBill.Business.DTOs
{
    public class RoomDto
    {
        public int Id { get; set; }
        public string RoomNumber { get; set; } = default!;
        public string? Block { get; set; }
        public int? Floor { get; set; }
        public decimal Price { get; set; }
        public string Status { get; set; } = default!;
    }
}
