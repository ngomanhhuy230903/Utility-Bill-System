using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;
using UtilityBill.WebApp.DTOs;
using UtilityBill.WebApp.Services;
using System.Text.Json;

namespace UtilityBill.WebApp.Pages.Invoices
{
    public class PayModel : PageModel
    {
        private readonly IApiClient _apiClient;
        private readonly IConfiguration _configuration;

        public PayModel(IApiClient apiClient, IConfiguration configuration)
        {
            _apiClient = apiClient;
            _configuration = configuration;
        }

        [BindProperty(SupportsGet = true)]
        public Guid Id { get; set; }

        public InvoiceDto? Invoice { get; set; }
        
        public string ApiBaseUrl => _configuration["ApiBaseUrl"] ?? "https://localhost:7240/api";

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


    }
} 