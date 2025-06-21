using Microsoft.AspNetCore.Mvc;
using UtilityBill.Api.Services.Momo;
using UtilityBill.Api.Services.VnPay;
using UtilityBill.Data.Models;
using UtilityBill.Data.Models.VnPay;

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


}
