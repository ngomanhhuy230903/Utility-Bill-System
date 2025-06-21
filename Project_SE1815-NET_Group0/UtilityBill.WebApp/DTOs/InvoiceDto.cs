namespace UtilityBill.WebApp.DTOs
{
    public class InvoiceDto
    {
        public Guid Id { get; set; }
        public int RoomId { get; set; }
        public RoomDto Room { get; set; } // Dùng lại RoomDto đã có
        public int InvoicePeriodMonth { get; set; }
        public int InvoicePeriodYear { get; set; }
        public DateTime DueDate { get; set; }
        public decimal TotalAmount { get; set; }
        public string Status { get; set; }
    }
}