namespace UtilityBill.Api.DTOs
{
    public class ConsumptionReportDto
    {
        public string? Period { get; set; }
        public decimal TotalElectric { get; set; }
        public decimal TotalWater { get; set; }
    }
}
