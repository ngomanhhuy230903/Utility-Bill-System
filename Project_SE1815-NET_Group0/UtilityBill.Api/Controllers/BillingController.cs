using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UtilityBill.Api.DTOs; // Thêm using này
using UtilityBill.Business.Interfaces;
using UtilityBill.Data.Repositories;
using System.Linq; // Thêm using này cho Select
using System.Threading.Tasks;

namespace UtilityBill.Api.Controllers
{
    [Authorize]
    public class BillingController : BaseApiController
    {
        private readonly IBillingService _billingService;
        private readonly IPdfService _pdfService;
        private readonly IUnitOfWork _unitOfWork;

        public BillingController(IBillingService billingService, IPdfService pdfService, IUnitOfWork unitOfWork)
        {
            _billingService = billingService;
            _pdfService = pdfService;
            _unitOfWork = unitOfWork;
        }

        [HttpPost("generate-invoices")]
        public async Task<IActionResult> GenerateInvoices()
        {
            await _billingService.GenerateInvoicesForPreviousMonthAsync();
            return Ok("Quá trình tạo hóa đơn đã được kích hoạt và đang chạy trong nền.");
        }

        // === SỬA LẠI ACTION NÀY ===
        [HttpGet] // GET: api/billing
        public async Task<IActionResult> GetAllInvoices()
        {
            var invoicesFromDb = await _unitOfWork.InvoiceRepository.GetAllInvoicesWithRoomAsync();

            // Chuyển đổi từ List<Invoice> (Entity) sang List<InvoiceDto>
            var invoiceDtos = invoicesFromDb.Select(invoice => new InvoiceDto
            {
                Id = invoice.Id,
                RoomId = invoice.RoomId,
                InvoicePeriodMonth = invoice.InvoicePeriodMonth,
                InvoicePeriodYear = invoice.InvoicePeriodYear,
                DueDate = invoice.DueDate,
                TotalAmount = invoice.TotalAmount,
                Status = invoice.Status,
                // Tạo RoomDto con từ đối tượng Room đã được Include
                Room = new RoomDto
                {
                    Id = invoice.Room.Id,
                    RoomNumber = invoice.Room.RoomNumber,
                    Block = invoice.Room.Block,
                    Floor = invoice.Room.Floor,
                    Area = invoice.Room.Area,
                    Price = invoice.Room.Price,
                    Status = invoice.Room.Status,
                    CreatedAt = invoice.Room.CreatedAt
                }
            }).ToList();

            return Ok(invoiceDtos);
        }

        [HttpGet("{invoiceId}/pdf")]
        public async Task<IActionResult> GetInvoicePdf(Guid invoiceId)
        {
            var pdfBytes = await _pdfService.GenerateInvoicePdfAsync(invoiceId);

            if (pdfBytes == null)
            {
                return NotFound("Không tìm thấy hóa đơn hoặc thông tin liên quan.");
            }

            return File(pdfBytes, "application/pdf", $"HoaDon-{invoiceId.ToString().Substring(0, 8).ToUpper()}.pdf");
        }
    }
}