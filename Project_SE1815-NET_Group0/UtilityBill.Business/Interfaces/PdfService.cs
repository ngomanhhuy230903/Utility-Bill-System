using UtilityBill.Business.Interfaces;
using UtilityBill.Business.PdfGenerator;
using UtilityBill.Data.Repositories;
using QuestPDF.Fluent;
namespace UtilityBill.Business.Services
{
    public class PdfService : IPdfService
    {
        private readonly IUnitOfWork _unitOfWork;

        public PdfService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<byte[]?> GenerateInvoicePdfAsync(Guid invoiceId)
        {
            // 1. Lấy dữ liệu hóa đơn đầy đủ
            var invoice = await _unitOfWork.InvoiceRepository.GetInvoiceWithDetailsAsync(invoiceId);
            if (invoice == null) return null;

            // 2. Lấy thông tin người thuê hiện tại của phòng đó
            var currentTenantHistory = await _unitOfWork.TenantHistoryRepository.GetCurrentHistoryByRoomIdAsync(invoice.RoomId);
            if (currentTenantHistory == null) return null; // Không có người thuê để xuất hóa đơn

            var tenant = await _unitOfWork.UserRepository.GetUserByIdAsync(currentTenantHistory.TenantId);
            if (tenant == null) return null;

            // 3. Tạo document và generate PDF
            var document = new InvoiceDocument(invoice, tenant);
            byte[] pdfBytes = document.GeneratePdf();

            return pdfBytes;
        }
    }
}