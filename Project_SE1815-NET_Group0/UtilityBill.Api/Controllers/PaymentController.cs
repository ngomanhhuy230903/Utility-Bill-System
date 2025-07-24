using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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
public class PaymentController(
    IMomoService momoService,
    IVnPayService vnPayService,
    IPushNotificationService pushNotificationService,
    ILogger<PaymentController> logger,
    UtilityBillDbContext dbContext)
    : Controller
{
    [HttpPost("CreatePaymentUrlVnpay")]
    public IActionResult CreatePaymentUrlVnpay(PaymentInformationModel model)
    {
        try
        {
            var url = vnPayService.CreatePaymentUrl(model, HttpContext);

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
        var response = vnPayService.PaymentExecute(Request.Query);

        // Process successful VnPay payment
        if (response.Success)
        {
            try
            {
                // Find and update the pending payment record
                var pendingPayment = await dbContext.Payments
                    .FirstOrDefaultAsync(p => p.Status == "Pending" && 
                                             p.PaymentMethod == PaymentMethod.VNPAY &&
                                             p.InvoiceId.ToString() == response.OrderId);

                if (pendingPayment != null)
                {
                    // Update payment status
                    pendingPayment.Status = "Success";
                    pendingPayment.TransactionCode = response.TransactionId;
                    await dbContext.SaveChangesAsync();

                    // Update invoice status to Paid
                    var invoice = await dbContext.Invoices.FindAsync(pendingPayment.InvoiceId);
                    if (invoice != null)
                    {
                        invoice.Status = "Paid";
                        await dbContext.SaveChangesAsync();
                        logger.LogInformation("Invoice {InvoiceId} marked as Paid for VnPay payment", invoice.Id);
                    }

                    // Send success notification
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

                    await pushNotificationService.SendNotificationAsync(notification);
                    logger.LogInformation("Payment success notification sent for VnPay transaction {TransactionId}",
                        response.TransactionId);
                }
                else
                {
                    logger.LogWarning("No pending VnPay payment found for OrderId: {OrderId}", response.OrderId);
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex,
                    "Failed to process VnPay payment success for OrderId: {OrderId}", response.OrderId);
            }
        }

        return Json(response);
    }

    [HttpGet("PaymentCallBack")]
    public async Task<IActionResult> PaymentCallBack()
    {
        var response = momoService.PaymentExecuteAsync(Request.Query);

        // Process successful MoMo payment
        if (response != null && !string.IsNullOrEmpty(response.OrderId))
        {
            try
            {
                // Find and update the pending payment record
                var pendingPayment = await dbContext.Payments
                    .FirstOrDefaultAsync(p => p.Status == "Pending" && 
                                             p.PaymentMethod == PaymentMethod.MOMO &&
                                             p.InvoiceId.ToString() == response.OrderId);

                if (pendingPayment != null)
                {
                    // Update payment status
                    pendingPayment.Status = "Success";
                    pendingPayment.TransactionCode = response.OrderId; // MoMo uses OrderId as transaction code
                    await dbContext.SaveChangesAsync();

                    // Update invoice status to Paid
                    var invoice = await dbContext.Invoices.FindAsync(pendingPayment.InvoiceId);
                    if (invoice != null)
                    {
                        invoice.Status = "Paid";
                        await dbContext.SaveChangesAsync();
                        logger.LogInformation("Invoice {InvoiceId} marked as Paid for MoMo payment", invoice.Id);
                    }

                    // Send success notification
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

                    await pushNotificationService.SendNotificationAsync(notification);
                    logger.LogInformation("Payment success notification sent for MoMo order {OrderId}", response.OrderId);
                }
                else
                {
                    logger.LogWarning("No pending MoMo payment found for OrderId: {OrderId}", response.OrderId);
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to process MoMo payment success for OrderId: {OrderId}",
                    response.OrderId);
            }
        }

        return Json(response);
    }

    [HttpPost("CreatePaymentMomo")]
    public async Task<IActionResult> CreatePaymentMomo(OrderInfo model)
    {
        try
        {
            var response = await momoService.CreatePaymentMomo(model);
            
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
            logger.LogInformation(
                "CreateUnifiedPayment called with paymentMethod: {PaymentMethod}, OrderId: {OrderId}", paymentMethod,
                request?.OrderId);

            object paymentResponse = null;
            var isSuccess = false;
            var message = string.Empty;

            // Validate request
            if (request == null)
            {
                logger.LogError("Request is null");
                return BadRequest(new { message = "Request data is required" });
            }

            switch (paymentMethod)
            {
                case PaymentMethod.VNPAY:
                {
                    var vnpayModel = request.ToPaymentInformationModel();
                    var url = vnPayService.CreatePaymentUrl(vnpayModel, HttpContext);
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
                    var response = await momoService.CreatePaymentMomo(momoModel);
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

                    await pushNotificationService.SendNotificationAsync(warningNotification);
                    logger.LogInformation("Cash payment warning notification sent for order {OrderId}",
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

                    await pushNotificationService.SendNotificationAsync(successNotification);
                    logger.LogInformation("Payment success notification sent for {PaymentMethod} order {OrderId}",
                        paymentMethod, request.OrderId);
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to send payment notification for {PaymentMethod} order {OrderId}",
                    paymentMethod, request.OrderId);
            }

            return Ok(new { message, paymentResponse });
        }

        catch (Exception ex)
        {
            logger.LogError(ex, "Error in CreateUnifiedPayment for paymentMethod: {PaymentMethod}, OrderId: {OrderId}",
                paymentMethod, request?.OrderId);
            
            // Send notification for failed payment
            try
            {
                var failedNotification = new PushNotificationDto
                {
                    Title = "Payment Failed",
                    Body = $"Payment for {paymentMethod} failed: {ex.Message}",
                    Tag = "payment-failed",
                    Data = JsonSerializer.Serialize(new
                    {
                        type = "payment_failed",
                        paymentMethod = paymentMethod.ToString(),
                        orderId = request?.OrderId,
                        error = ex.Message
                    })
                };

                await pushNotificationService.SendNotificationAsync(failedNotification);
            }
            catch (Exception notificationEx)
            {
                logger.LogError(notificationEx, "Failed to send payment failure notification");
            }
            
            return StatusCode(500, new { message = "An error occurred while processing the payment: " + ex.Message });
        }
    }

    [HttpPost("mark-payment-successful")]
    public async Task<IActionResult> MarkPaymentSuccessful([FromBody] MarkPaymentSuccessfulRequest request)
    {
        try
        {
            var payment = await dbContext.Payments.FindAsync(request.PaymentId);
            if (payment == null)
                return NotFound("Payment not found");

            payment.Status = "Success";
            payment.TransactionCode = request.TransactionCode;
            await dbContext.SaveChangesAsync();

            // Update invoice status to Paid
            var invoice = await dbContext.Invoices.FindAsync(payment.InvoiceId);
            if (invoice != null)
            {
                invoice.Status = "Paid";
                await dbContext.SaveChangesAsync();
                logger.LogInformation("Invoice {InvoiceId} marked as Paid for payment {PaymentId}", invoice.Id, payment.Id);
            }

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

                await pushNotificationService.SendNotificationAsync(notification);
                logger.LogInformation("Payment confirmation notification sent for payment {PaymentId}", payment.Id);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to send payment confirmation notification for payment {PaymentId}",
                    payment.Id);
            }

            return Ok(new { message = "Payment marked as successful", payment });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error marking payment as successful");
            return StatusCode(500, "Internal server error");
        }
    }
}

public class MarkPaymentSuccessfulRequest
{
    public Guid PaymentId { get; set; }
    public string? TransactionCode { get; set; }
}