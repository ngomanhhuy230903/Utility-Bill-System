namespace UtilityBill.WebApp.DTOs
{
    public class ConsumptionReportDto
    {
        public string? Period { get; set; }  // ex: "05/2025", "2025", "05/05/2025"
        public decimal TotalElectric { get; set; }
        public decimal TotalWater { get; set; }
    }
}
