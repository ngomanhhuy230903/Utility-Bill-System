// File: UtilityBill.Api/Controllers/BillingController.cs

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UtilityBill.Business.Interfaces;
using System;
using System.Threading.Tasks;

namespace UtilityBill.Api.Controllers
{
    //[Authorize]
    public class BillingController : BaseApiController
    {
        private readonly IBillingService _billingService;
        private readonly IPdfService _pdfService; // Thêm dịch vụ PDF

        // Sửa constructor để nhận cả 2 service
        public BillingController(IBillingService billingService, IPdfService pdfService)
        {
            _billingService = billingService;
            _pdfService = pdfService;
        }

        [HttpPost("generate-invoices")]
        public async Task<IActionResult> GenerateInvoices()
        {
            await _billingService.GenerateInvoicesForPreviousMonthAsync();
            return Ok("Quá trình tạo hóa đơn đã được kích hoạt và đang chạy trong nền.");
        }

        [HttpGet("{invoiceId}/pdf")]
        public async Task<IActionResult> GetInvoicePdf(Guid invoiceId)
        {
            var pdfBytes = await _pdfService.GenerateInvoicePdfAsync(invoiceId);

            if (pdfBytes == null)
            {
                return NotFound("Không tìm thấy hóa đơn hoặc thông tin liên quan.");
            }

            // LỖI ĐƯỢC SỬA Ở ĐÂY: Dùng 'invoiceId' thay vì 'invoice.Id'
            return File(pdfBytes, "application/pdf", $"HoaDon-{invoiceId.ToString().Substring(0, 8).ToUpper()}.pdf");
        }
    }
}