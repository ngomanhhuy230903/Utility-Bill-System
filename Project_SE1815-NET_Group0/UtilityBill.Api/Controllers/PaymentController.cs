using Microsoft.AspNetCore.Mvc;
using UtilityBill.Api.Services.Momo;
using UtilityBill.Api.Services.VnPay;
using UtilityBill.Data.Models;
using UtilityBill.Data.Models.VnPay;
using UtilityBill.Data.Enums;
using UtilityBill.Data;
using Microsoft.EntityFrameworkCore;
using UtilityBill.Api.DTOs;

namespace UtilityBill.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PaymentController : Controller
{
    private IMomoService _momoService;
    private readonly IVnPayService _vnPayService;
    public PaymentController(IMomoService momoService,IVnPayService vnPayService)
    {
        _momoService = momoService;
        _vnPayService = vnPayService;
    }

    [HttpPost("CreatePaymentUrlVnpay")]
    public IActionResult CreatePaymentUrlVnpay(PaymentInformationModel model)
    {
        try
        {
            var url = _vnPayService.CreatePaymentUrl(model, HttpContext);
            
            // Check if the URL is valid
            if (string.IsNullOrEmpty(url))
            {
                return BadRequest("VnPay service returned null or empty URL");
            }
            
            // Check if the URL looks like a valid VnPay URL
            if (!url.Contains("vnpayment.vn"))
            {
                return BadRequest($"Invalid VnPay URL generated: {url}");
            }
            
            // Return JSON response with the redirect URL instead of redirecting directly
            return Json(new { redirectUrl = url });
        }
        catch (Exception ex)
        {
            return BadRequest($"VnPay payment error: {ex.Message}");
        }
    }
    [HttpGet("PaymentCallbackVnpay")]
    public IActionResult PaymentCallbackVnpay()
    {
        var response = _vnPayService.PaymentExecute(Request.Query);

        return Json(response);
    }
    [HttpPost("CreatePaymentMomo")]
    public async Task<IActionResult> CreatePaymentMomo(OrderInfo model)
    {
        try
        {
            var response = await _momoService.CreatePaymentMomo(model);
            
            // Check if the response and PayUrl are valid
            if (response == null)
            {
                return BadRequest("MoMo service returned null response");
            }
            
            if (string.IsNullOrEmpty(response.PayUrl))
            {
                // Log the error details
                var errorMessage = $"MoMo payment failed. ErrorCode: {response.ErrorCode}, Message: {response.Message}, LocalMessage: {response.LocalMessage}";
                return BadRequest(errorMessage);
            }
            
            return Redirect(response.PayUrl);
        }
        catch (Exception ex)
        {
            return BadRequest($"MoMo payment error: {ex.Message}");
        }
    }

    [HttpPost("/api/payments/create")]
    public async Task<IActionResult> CreateUnifiedPayment([FromBody] UnifiedPaymentRequest request, [FromQuery] PaymentMethod paymentMethod)
    {
        object paymentResponse = null;
        bool isSuccess = false;
        string message = string.Empty;

        // Get db context (assume injected via DI, otherwise adjust as needed)
        var dbContext = HttpContext.RequestServices.GetService(typeof(UtilityBill.Data.Context.UtilityBillDbContext)) as UtilityBill.Data.Context.UtilityBillDbContext;
        if (dbContext == null)
            return StatusCode(500, "Database context not available");

        switch (paymentMethod)
        {
            case PaymentMethod.VNPAY:
            {
                var vnpayModel = request.ToPaymentInformationModel();
                var url = _vnPayService.CreatePaymentUrl(vnpayModel, HttpContext);
                paymentResponse = new { redirectUrl = url };
                isSuccess = !string.IsNullOrEmpty(url) && url.Contains("vnpayment.vn");
                message = isSuccess ? "VnPay payment URL generated successfully." : "Failed to generate VnPay payment URL.";
                break;
            }
            case PaymentMethod.MOMO:
            {
                var momoModel = request.ToOrderInfo();
                var response = await _momoService.CreatePaymentMomo(momoModel);
                paymentResponse = response;
                isSuccess = response != null && !string.IsNullOrEmpty(response.PayUrl);
                message = isSuccess ? "MoMo payment URL generated successfully." : "Failed to generate MoMo payment URL.";
                break;
            }
            case PaymentMethod.CASH:
            {
                // Create and save payment with Unpaid status
                var payment = new Payment
                {
                    Id = Guid.NewGuid(),
                    InvoiceId = Guid.TryParse(request.OrderId, out var invoiceId) ? invoiceId : Guid.Empty,
                    PaymentDate = DateTime.UtcNow,
                    Amount = (decimal)request.Amount,
                    PaymentMethod = paymentMethod,
                    Status = "Unpaid"
                };
                dbContext.Payments.Add(payment);
                await dbContext.SaveChangesAsync();
                paymentResponse = payment;
                isSuccess = true;
                message = "Cash payment recorded as Unpaid.";
                break;
            }
            case PaymentMethod.STRIPE:
            {
                throw new NotImplementedException("Stripe payment is not implemented yet.");
            }
            // Add more payment methods here as needed
            default:
                return BadRequest("Unsupported payment method.");
        }

        if (isSuccess)
        {
            // Map UnifiedPaymentRequest to Payment entity
            var payment = new Payment
            {
                Id = Guid.NewGuid(),
                InvoiceId = Guid.TryParse(request.OrderId, out var invoiceId) ? invoiceId : Guid.Empty,
                PaymentDate = DateTime.UtcNow,
                Amount = (decimal)request.Amount,
                PaymentMethod = paymentMethod,
                Status = "Success"
            };
            dbContext.Payments.Add(payment);
            await dbContext.SaveChangesAsync();
        }

        return Ok(new { message, paymentResponse });
    }
}
