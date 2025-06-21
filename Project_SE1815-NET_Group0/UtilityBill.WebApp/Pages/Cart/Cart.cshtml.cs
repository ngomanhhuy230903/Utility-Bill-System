using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace UtilityBill.WebApp.Pages.Cart;

public class Cart : PageModel
{
    private readonly IHttpClientFactory _httpClientFactory;

    public Cart(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    public decimal GrandTotal { get; set; } = 10000; // Default amount
    public string UserName { get; set; } = "Pham Quang"; // Default user name

    public void OnGet()
    {
        // Simple initialization without complex logic
        UserName = "Pham Quang";
        GrandTotal = 10000;
        
        // Only try to get user info if authenticated
        if (User.Identity?.IsAuthenticated == true)
        {
            var fullNameClaim = User.FindFirst("FullName");
            if (!string.IsNullOrEmpty(fullNameClaim?.Value))
            {
                UserName = fullNameClaim.Value;
            }
            else if (!string.IsNullOrEmpty(User.Identity?.Name))
            {
                UserName = User.Identity.Name;
            }
        }
    }

    [HttpPost]
    public async Task<IActionResult> OnPostCreatePaymentMomoAsync(string fullName, decimal amount, string orderInfo)
    {
        try
        {
            var client = _httpClientFactory.CreateClient("ApiClient");
            
            // Get user's full name using the same logic as OnGet
            string userName = GetUserName();
            
            // Create the correct model for MoMo API
            var paymentData = new
            {
                FullName = string.IsNullOrEmpty(fullName) ? userName : fullName,
                OrderId = Guid.NewGuid().ToString(), // Generate unique order ID
                OrderInformation = string.IsNullOrEmpty(orderInfo) ? "Thanh toán đặt hàng qua Momo tại UtilityStore" : orderInfo,
                Amount = amount <= 0 ? 10000.0 : (double)amount
            };

            var json = JsonSerializer.Serialize(paymentData);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            Console.WriteLine($"MoMo Payment Request: {json}");
            var response = await client.PostAsync("Payment/CreatePaymentMomo", content);
            
            Console.WriteLine($"MoMo Response Status: {response.StatusCode}");
            
            if (response.IsSuccessStatusCode)
            {
                // The API returns a redirect, so we need to get the redirect URL
                var redirectUrl = response.RequestMessage?.RequestUri?.ToString();
                if (string.IsNullOrEmpty(redirectUrl))
                {
                    // Try to get the URL from the response headers
                    var locationHeader = response.Headers.Location?.ToString();
                    if (!string.IsNullOrEmpty(locationHeader))
                    {
                        return Redirect(locationHeader);
                    }
                }
                
                // If we can't get the redirect URL, try to read the response content
                var responseContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"MoMo Response Content: {responseContent}");
                
                // If the response contains a URL, try to extract it
                if (responseContent.Contains("http"))
                {
                    // Simple URL extraction - look for URLs in the response
                    var urlMatch = System.Text.RegularExpressions.Regex.Match(responseContent, @"https?://[^\s""']+");
                    if (urlMatch.Success)
                    {
                        return Redirect(urlMatch.Value);
                    }
                }
                
                return BadRequest("Could not extract payment URL from response");
            }
            
            var errorContent = await response.Content.ReadAsStringAsync();
            return BadRequest($"Payment creation failed. Status: {response.StatusCode}, Content: {errorContent}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"MoMo Payment Error: {ex.Message}");
            return BadRequest($"Error: {ex.Message}");
        }
    }

    [HttpPost]
    public async Task<IActionResult> OnPostCreatePaymentUrlVnpayAsync(string fullName, decimal amount, string orderInfo)
    {
        try
        {
            var client = _httpClientFactory.CreateClient("ApiClient");
            
            // Get user's full name using the same logic as OnGet
            string userName = GetUserName();
            
            // Create the correct model for VnPay API
            var paymentData = new
            {
                OrderType = "other",
                Amount = amount <= 0 ? 10000.0 : (double)amount,
                OrderDescription = string.IsNullOrEmpty(orderInfo) ? "Thanh toán đặt hàng qua VnPay tại UtilityStore" : orderInfo,
                Name = string.IsNullOrEmpty(fullName) ? userName : fullName
            };

            var json = JsonSerializer.Serialize(paymentData);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            Console.WriteLine($"VnPay Payment Request: {json}");
            var response = await client.PostAsync("Payment/CreatePaymentUrlVnpay", content);
            
            Console.WriteLine($"VnPay Response Status: {response.StatusCode}");
            
            if (response.IsSuccessStatusCode)
            {
                // Read the JSON response
                var responseContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"VnPay Response Content: {responseContent}");
                
                try
                {
                    // Try to parse the JSON response
                    var jsonResponse = JsonSerializer.Deserialize<JsonElement>(responseContent);
                    
                    if (jsonResponse.TryGetProperty("redirectUrl", out var redirectUrlElement))
                    {
                        var redirectUrl = redirectUrlElement.GetString();
                        Console.WriteLine($"VnPay Redirect URL: {redirectUrl}");
                        
                        if (!string.IsNullOrEmpty(redirectUrl) && redirectUrl.Contains("vnpayment.vn"))
                        {
                            return Redirect(redirectUrl);
                        }
                        else
                        {
                            return BadRequest($"Invalid redirect URL in response: {redirectUrl}");
                        }
                    }
                    else
                    {
                        return BadRequest("No redirectUrl found in JSON response");
                    }
                }
                catch (JsonException ex)
                {
                    Console.WriteLine($"JSON parsing error: {ex.Message}");
                    return BadRequest($"Invalid JSON response from VnPay API: {responseContent}");
                }
            }
            
            var errorContent = await response.Content.ReadAsStringAsync();
            return BadRequest($"VnPay payment failed. Status: {response.StatusCode}, Content: {errorContent}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"VnPay Payment Error: {ex.Message}");
            return BadRequest($"Error: {ex.Message}");
        }
    }

    // Helper method to get user name with fallback logic
    private string GetUserName()
    {
        // Try to get the user's full name from claims first
        var fullNameClaim = User.FindFirst("FullName");
        if (!string.IsNullOrEmpty(fullNameClaim?.Value))
        {
            return fullNameClaim.Value;
        }
        // Fallback to User.Identity.Name if FullName claim is not available
        else if (!string.IsNullOrEmpty(User.Identity?.Name))
        {
            return User.Identity.Name;
        }
        // Final fallback to default value
        else
        {
            return "Pham Quang";
        }
    }
}