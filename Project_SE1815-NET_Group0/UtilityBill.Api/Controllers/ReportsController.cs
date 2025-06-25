using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OfficeOpenXml;
using UtilityBill.Business.Services;

namespace UtilityBill.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReportsController : ControllerBase
    {
        private readonly IReportService _reportService;

        public ReportsController(IReportService reportService)
        {
            _reportService = reportService;
        }

        [HttpGet("consumption")]
        public async Task<IActionResult> GetConsumptionReport(DateTime from, DateTime to, string groupBy)
        {
            var data = await _reportService.GetConsumptionReport(from, to, groupBy);
            return Ok(data);
        }

        [HttpGet("export-consumption")]
        public async Task<IActionResult> GetConsumptionReportExport(DateTime from, DateTime to, string groupBy)
        {
            var data = await _reportService.GetConsumptionReport(from, to, groupBy);

            ExcelPackage.License.SetNonCommercialPersonal("My Personal Use");
            using var package = new ExcelPackage();
            var ws = package.Workbook.Worksheets.Add("Consumption");
            ws.Cells.LoadFromCollection(data, true);

            return File(package.GetAsByteArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "consumption.xlsx");
        }

        [HttpGet("revenue")]
        public async Task<IActionResult> GetRevenueReport(DateTime from, DateTime to, string groupBy)
        {
            var data = await _reportService.GetRevenueReport(from, to, groupBy);
            return Ok(data);
        }

        [HttpGet("export-revenue")]
        public async Task<IActionResult> GetRevenueReportExport(DateTime from, DateTime to, string groupBy)
        {
            var data = await _reportService.GetRevenueReport(from, to, groupBy);

            ExcelPackage.License.SetNonCommercialPersonal("My Personal Use");
            using var package = new ExcelPackage();
            var ws = package.Workbook.Worksheets.Add("Revenue");
            ws.Cells.LoadFromCollection(data, true);

            return File(package.GetAsByteArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "revenue.xlsx");
        }
    }
}
