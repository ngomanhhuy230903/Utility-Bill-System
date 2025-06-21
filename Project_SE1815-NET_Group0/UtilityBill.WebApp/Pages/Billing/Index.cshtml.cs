using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using UtilityBill.WebApp.DTOs;
using UtilityBill.WebApp.Services;

namespace UtilityBill.WebApp.Pages.Billing
{
    [Authorize]
    public class IndexModel : PageModel
    {
        private readonly IApiClient _apiClient;
        public string ApiBaseUrl { get; }

        public IndexModel(IApiClient apiClient, IConfiguration configuration)
        {
            _apiClient = apiClient;
            // Lấy URL của API để tạo link PDF
            ApiBaseUrl = configuration["ApiBaseUrl"];
        }

        public List<InvoiceDto> Invoices { get; set; } = new List<InvoiceDto>();

        public async Task OnGetAsync()
        {
            Invoices = await _apiClient.GetInvoicesAsync();
        }

        public async Task<IActionResult> OnPostGenerateAsync()
        {
            await _apiClient.TriggerInvoiceGenerationAsync();
            // Tải lại trang để thấy hóa đơn mới
            return RedirectToPage();
        }
    }
}