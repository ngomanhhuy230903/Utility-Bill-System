using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using UtilityBill.Api.DTOs;
using UtilityBill.Api.Services.Momo;
using UtilityBill.Api.Services.VnPay;
using UtilityBill.Business.Interfaces;
using UtilityBill.Data.Context;
using UtilityBill.Data.DTOs;
using UtilityBill.Data.Enums;
using UtilityBill.Data.Models;
using UtilityBill.Data.Models.VnPay;

namespace UtilityBill.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PaymentController : Controller
{
    private readonly ILogger<PaymentController> _logger;
    private readonly IPushNotificationService _pushNotificationService;
    private readonly IVnPayService _vnPayService;
    private readonly IMomoService _momoService;

    public PaymentController(
        IMomoService momoService,
        IVnPayService vnPayService,
        IPushNotificationService pushNotificationService,
        ILogger<PaymentController> logger)
    {
        _momoService = momoService;
        _vnPayService = vnPayService;
        _pushNotificationService = pushNotificationService;
        _logger = logger;
    }

    [HttpPost("CreatePaymentUrlVnpay")]
    public IActionResult CreatePaymentUrlVnpay(PaymentInformationModel model)
    {
        try
        {
            var url = _vnPayService.CreatePaymentUrl(model, HttpContext);

            // Check if the URL is valid
            if (string.IsNullOrEmpty(url)) return BadRequest("VnPay service returned null or empty URL");

            // Check if the URL looks like a valid VnPay URL
            if (!url.Contains("vnpayment.vn")) return BadRequest($"Invalid VnPay URL generated: {url}");

            // Return JSON response with the redirect URL instead of redirecting directly
            return Json(new { redirectUrl = url });
        }
        catch (Exception ex)
        {
            return BadRequest($"VnPay payment error: {ex.Message}");
        }
    }

    [HttpGet("PaymentCallbackVnpay")]
    public async Task<IActionResult> PaymentCallbackVnpay()
    {
        var response = _vnPayService.PaymentExecute(Request.Query);

        // Send notification for successful VnPay payment
        if (response.Success)
            try
            {
                var notification = new PushNotificationDto
                {
                    Title = "Payment Successful!",
                    Body =
                        $"Your VnPay payment of {response.OrderDescription} has been completed successfully. Transaction ID: {response.TransactionId}",
                    Tag = "payment-success",
                    Data = JsonSerializer.Serialize(new
                    {
                        type = "payment_success",
                        paymentMethod = "VnPay",
                        transactionId = response.TransactionId,
                        orderId = response.OrderId
                    })
                };

                await _pushNotificationService.SendNotificationAsync(notification);
                _logger.LogInformation("Payment success notification sent for VnPay transaction {TransactionId}",
                    response.TransactionId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Failed to send payment success notification for VnPay transaction {TransactionId}",
                    response.TransactionId);
            }

        return Json(response);
    }

    [HttpGet("PaymentCallBack")]
    public async Task<IActionResult> PaymentCallBack()
    {
        var response = _momoService.PaymentExecuteAsync(Request.Query);

        // Send notification for successful MoMo payment
        if (response != null && !string.IsNullOrEmpty(response.OrderId))
            try
            {
                var notification = new PushNotificationDto
                {
                    Title = "Payment Successful!",
                    Body = $"Your MoMo payment has been completed successfully. Order ID: {response.OrderId}",
                    Tag = "payment-success",
                    Data = JsonSerializer.Serialize(new
                    {
                        type = "payment_success",
                        paymentMethod = "MoMo",
                        orderId = response.OrderId,
                        amount = response.Amount
                    })
                };

                await _pushNotificationService.SendNotificationAsync(notification);
                _logger.LogInformation("Payment success notification sent for MoMo order {OrderId}", response.OrderId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send payment success notification for MoMo order {OrderId}",
                    response.OrderId);
            }

        return Json(response);
    }

    [HttpPost("CreatePaymentMomo")]
    public async Task<IActionResult> CreatePaymentMomo(OrderInfo model)
    {
        try
        {
            var response = await _momoService.CreatePaymentMomo(model);

            // Check if the response and PayUrl are valid
            if (response == null) return BadRequest("MoMo service returned null response");

            if (string.IsNullOrEmpty(response.PayUrl))
            {
                // Log the error details
                var errorMessage =
                    $"MoMo payment failed. ErrorCode: {response.ErrorCode}, Message: {response.Message}, LocalMessage: {response.LocalMessage}";
                return BadRequest(errorMessage);
            }

            return Redirect(response.PayUrl);
        }
        catch (Exception ex)
        {
            return BadRequest($"MoMo payment error: {ex.Message}");
        }
    }


    [HttpPost("create")]
    public async Task<IActionResult> CreateUnifiedPayment([FromBody] UnifiedPaymentRequest request,
        [FromQuery] PaymentMethod paymentMethod)
    {
        try
        {
            _logger.LogInformation(
                "CreateUnifiedPayment called with paymentMethod: {PaymentMethod}, OrderId: {OrderId}", paymentMethod,
                request?.OrderId);

            object paymentResponse = null;
            var isSuccess = false;
            var message = string.Empty;

            // Validate request
            if (request == null)
            {
                _logger.LogError("Request is null");
                return BadRequest(new { message = "Request data is required" });
            }

            // Get db context (assume injected via DI, otherwise adjust as needed)
            var dbContext =
                HttpContext.RequestServices.GetService(typeof(UtilityBillDbContext)) as UtilityBillDbContext;
            if (dbContext == null)
            {
                _logger.LogError("Database context not available");
                return StatusCode(500, new { message = "Database context not available" });
            }

            switch (paymentMethod)
            {
                case PaymentMethod.VNPAY:
                {
                    var vnpayModel = request.ToPaymentInformationModel();
                    var url = _vnPayService.CreatePaymentUrl(vnpayModel, HttpContext);
                    paymentResponse = new { redirectUrl = url };
                    isSuccess = !string.IsNullOrEmpty(url) && url.Contains("vnpayment.vn");
                    message = isSuccess
                        ? "VnPay payment URL generated successfully."
                        : "Failed to generate VnPay payment URL.";
                    break;
                }
                case PaymentMethod.MOMO:
                {
                    var momoModel = request.ToOrderInfo();
                    var response = await _momoService.CreatePaymentMomo(momoModel);
                    paymentResponse = response;
                    isSuccess = response != null && !string.IsNullOrEmpty(response.PayUrl);
                    message = isSuccess
                        ? "MoMo payment URL generated successfully."
                        : "Failed to generate MoMo payment URL.";
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
                    message = "Cash payment recorded successfully.";
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

            if (isSuccess && paymentMethod != PaymentMethod.CASH)
            {
                // For online payments (VnPay, MoMo), create a pending payment record
                // The actual payment record will be created when the payment callback is received
                var payment = new Payment
                {
                    Id = Guid.NewGuid(),
                    InvoiceId = Guid.TryParse(request.OrderId, out var invoiceId) ? invoiceId : Guid.Empty,
                    PaymentDate = DateTime.UtcNow,
                    Amount = (decimal)request.Amount,
                    PaymentMethod = paymentMethod,
                    Status = "Pending"
                };
                dbContext.Payments.Add(payment);
                await dbContext.SaveChangesAsync();
            }

            // Send appropriate notifications based on payment method
            try
            {
                if (paymentMethod == PaymentMethod.CASH)
                {
                    // Send warning notification for cash payment
                    var warningNotification = new PushNotificationDto
                    {
                        Title = "Cash Payment Warning",
                        Body =
                            $"Cash payment of {request.Amount:N0} VND has been recorded as 'Unpaid'. Please complete the payment in person.",
                        Tag = "cash-payment-warning",
                        Data = JsonSerializer.Serialize(new
                        {
                            type = "cash_payment_warning",
                            paymentMethod = "Cash",
                            amount = request.Amount,
                            orderId = request.OrderId
                        })
                    };

                    await _pushNotificationService.SendNotificationAsync(warningNotification);
                    _logger.LogInformation("Cash payment warning notification sent for order {OrderId}",
                        request.OrderId);
                }
                else
                {
                    // Send success notification for online payments
                    var successNotification = new PushNotificationDto
                    {
                        Title = "Payment Successful!",
                        Body =
                            $"Your {paymentMethod} payment of {request.Amount:N0} VND has been processed successfully.",
                        Tag = "payment-success",
                        Data = JsonSerializer.Serialize(new
                        {
                            type = "payment_success",
                            paymentMethod = paymentMethod.ToString(),
                            amount = request.Amount,
                            orderId = request.OrderId
                        })
                    };

                    await _pushNotificationService.SendNotificationAsync(successNotification);
                    _logger.LogInformation("Payment success notification sent for {PaymentMethod} order {OrderId}",
                        paymentMethod, request.OrderId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send payment notification for {PaymentMethod} order {OrderId}",
                    paymentMethod, request.OrderId);
            }

            return Ok(new { message, paymentResponse });
        }

        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in CreateUnifiedPayment for paymentMethod: {PaymentMethod}, OrderId: {OrderId}",
                paymentMethod, request?.OrderId);
            return StatusCode(500, new { message = "An error occurred while processing the payment: " + ex.Message });
        }
    }

    [HttpPost("mark-payment-successful")]
    public async Task<IActionResult> MarkPaymentSuccessful([FromBody] MarkPaymentSuccessfulRequest request)
    {
        try
        {
            var dbContext =
                HttpContext.RequestServices.GetService(typeof(UtilityBillDbContext)) as UtilityBillDbContext;
            if (dbContext == null)
                return StatusCode(500, "Database context not available");

            var payment = await dbContext.Payments.FindAsync(request.PaymentId);
            if (payment == null)
                return NotFound("Payment not found");

            payment.Status = "Success";
            payment.TransactionCode = request.TransactionCode;
            await dbContext.SaveChangesAsync();

            // Send success notification
            try
            {
                var notification = new PushNotificationDto
                {
                    Title = "Payment Confirmed!",
                    Body =
                        $"Your {payment.PaymentMethod} payment of {payment.Amount:N0} VND has been confirmed and processed successfully.",
                    Tag = "payment-confirmed",
                    Data = JsonSerializer.Serialize(new
                    {
                        type = "payment_confirmed",
                        paymentMethod = payment.PaymentMethod.ToString(),
                        amount = payment.Amount,
                        transactionCode = payment.TransactionCode,
                        paymentId = payment.Id
                    })
                };

                await _pushNotificationService.SendNotificationAsync(notification);
                _logger.LogInformation("Payment confirmation notification sent for payment {PaymentId}", payment.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send payment confirmation notification for payment {PaymentId}",
                    payment.Id);
            }

            return Ok(new { message = "Payment marked as successful", payment });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error marking payment as successful");
            return StatusCode(500, "Internal server error");
        }
    }
}

public class MarkPaymentSuccessfulRequest
{
    public Guid PaymentId { get; set; }
    public string? TransactionCode { get; set; }
}