namespace UtilityBill.WebApp.DTOs
{
    public class InvoiceDetailDto
    {
        public int Id { get; set; }
        public string Description { get; set; } = string.Empty;
        public decimal Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal Amount { get; set; }
    }

    public class InvoiceDto
    {
        public Guid Id { get; set; }
        public int RoomId { get; set; }
        public RoomDto Room { get; set; } = new RoomDto();
        public int InvoicePeriodMonth { get; set; }
        public int InvoicePeriodYear { get; set; }
        public DateTime DueDate { get; set; }
        public decimal TotalAmount { get; set; }
        public string Status { get; set; } = string.Empty;
        public List<InvoiceDetailDto> InvoiceDetails { get; set; } = new List<InvoiceDetailDto>();
    }
}