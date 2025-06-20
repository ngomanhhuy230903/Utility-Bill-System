using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UtilityBill.Business.Interfaces;

namespace UtilityBill.Api.Controllers
{
    //[Authorize] // Có thể chỉ cho Admin dùng
    public class BillingController : BaseApiController
    {
        private readonly IBillingService _billingService;

        public BillingController(IBillingService billingService)
        {
            _billingService = billingService;
        }

        [HttpPost("generate-invoices")]
        public async Task<IActionResult> GenerateInvoices()
        {
            await _billingService.GenerateInvoicesForPreviousMonthAsync();
            return Ok("Quá trình tạo hóa đơn đã được kích hoạt và đang chạy trong nền.");
        }
    }
}