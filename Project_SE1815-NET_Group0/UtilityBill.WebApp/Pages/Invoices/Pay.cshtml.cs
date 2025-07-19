using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using UtilityBill.WebApp.DTOs;
using UtilityBill.WebApp.Services;
using System.Text.Json;

namespace UtilityBill.WebApp.Pages.Invoices
{
    public class PayModel : PageModel
    {
        private readonly IApiClient _apiClient;

        public PayModel(IApiClient apiClient)
        {
            _apiClient = apiClient;
        }

        [BindProperty(SupportsGet = true)]
        public Guid Id { get; set; }

        public InvoiceDto? Invoice { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            try
            {
                Invoice = await _apiClient.GetInvoiceByIdAsync(Id);
                
                if (Invoice == null)
                {
                    return NotFound();
                }

                return Page();
            }
            catch (Exception ex)
            {
                // Log the error (in a real application, you'd use a proper logging service)
                Console.WriteLine($"Error loading invoice {Id}: {ex.Message}");
                return NotFound();
            }
        }

        public async Task<IActionResult> OnPostPaymentAsync(string paymentMethod)
        {
            try
            {
                if (Invoice == null)
                {
                    Invoice = await _apiClient.GetInvoiceByIdAsync(Id);
                    if (Invoice == null)
                    {
                        return NotFound();
                    }
                }

                var paymentRequest = new
                {
                    orderId = Invoice.Id.ToString(),
                    amount = Invoice.TotalAmount,
                    orderDescription = $"Utility bill payment for Room {Invoice.Room.RoomNumber}",
                    name = User.Identity?.Name ?? "Guest",
                    orderType = "utility_bill"
                };

                var result = await _apiClient.CreateUnifiedPaymentAsync(paymentRequest, paymentMethod);
                
                if (result != null)
                {
                    return new JsonResult(new { success = true, data = result });
                }
                else
                {
                    return new JsonResult(new { success = false, message = "Payment failed" });
                }
            }
            catch (Exception ex)
            {
                return new JsonResult(new { success = false, message = ex.Message });
            }
        }
    }
} 