using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using UtilityBill.WebApp.Services;
using System;
using System.Threading.Tasks;

namespace UtilityBill.WebApp.Pages.Billing
{
    [Authorize]
    public class ViewPdfModel : PageModel
    {
        private readonly IApiClient _apiClient;

        public ViewPdfModel(IApiClient apiClient)
        {
            _apiClient = apiClient;
        }

        // Phương thức này sẽ được gọi khi bạn truy cập trang /Billing/ViewPdf?id=...
        public async Task<IActionResult> OnGetAsync(Guid id)
        {
            // Dùng ApiClient để gọi đến API một cách an toàn (có kèm token)
            var pdfBytes = await _apiClient.GetInvoicePdfAsync(id);

            if (pdfBytes == null || pdfBytes.Length == 0)
            {
                return NotFound("Không tìm thấy hóa đơn hoặc không thể tạo file PDF.");
            }

            // Trả về một file PDF trực tiếp cho trình duyệt
            return File(pdfBytes, "application/pdf");
        }
    }
}