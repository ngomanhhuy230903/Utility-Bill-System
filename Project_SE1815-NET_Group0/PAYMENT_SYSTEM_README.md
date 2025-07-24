# Payment System Documentation

## Overview

The Utility Bill Management System includes a comprehensive payment processing system that supports multiple payment methods including VnPay, MoMo, and Cash payments. The system automatically handles payment processing, status updates, and invoice management.

## ⚠️ **IMPORTANT NOTE: CreateUnifiedPayment Function**

**`CreateUnifiedPayment` is a CONTROLLER FUNCTION that receives POST HTTP requests - it is NOT a page.**

- **Location**: `PaymentController.CreateUnifiedPayment()`
- **Type**: API Controller Method
- **HTTP Method**: POST
- **Route**: `/api/payment/create`
- **Purpose**: Processes payment requests from the frontend and handles different payment methods
- **Invoice Integration**: Payment information is automatically applied to invoice data when payments are processed

## How the Payment System Works

### 1. **Payment Flow Overview**

```
Frontend (Pay.cshtml) → API Controller (CreateUnifiedPayment) → Payment Gateway → Callback → Status Update
```

### 2. **Payment Methods Supported**

#### **VnPay Payment**
- **Gateway**: VnPay payment gateway
- **Flow**: Create payment URL → Redirect to VnPay → Process callback → Update status
- **Status Tracking**: Pending → Success/Failed

#### **MoMo Payment**
- **Gateway**: MoMo payment gateway
- **Flow**: Create payment URL → Redirect to MoMo → Process callback → Update status
- **Status Tracking**: Pending → Success/Failed

#### **Cash Payment**
- **Type**: Manual payment recording
- **Flow**: Record payment → Mark as Unpaid → Manual confirmation → Update status
- **Status Tracking**: Unpaid → Success (after manual confirmation)

## Technical Implementation

### **Core Components**

#### **1. PaymentController**
```csharp
[ApiController]
[Route("api/[controller]")]
public class PaymentController : Controller
{
    // CreateUnifiedPayment - MAIN PAYMENT PROCESSING FUNCTION
    [HttpPost("create")]
    public async Task<IActionResult> CreateUnifiedPayment(
        [FromBody] UnifiedPaymentRequest request,
        [FromQuery] PaymentMethod paymentMethod)
    {
        // Processes payment requests automatically
        // NOT a page - this is an API endpoint
    }
}
```

#### **2. Payment Processing Flow**

**Step 1: Frontend Request**
```javascript
// From Pay.cshtml - sends POST request to API
const response = await fetch(apiUrl, {
    method: 'POST',
    headers: {
        'Content-Type': 'application/json',
        'Authorization': 'Bearer ' + token
    },
    body: JSON.stringify(paymentRequest)
});
```

**Step 2: Controller Processing**
```csharp
// CreateUnifiedPayment processes the payment request
switch (paymentMethod)
{
    case PaymentMethod.VNPAY:
        // Generate VnPay URL and redirect
        break;
    case PaymentMethod.MOMO:
        // Generate MoMo URL and redirect
        break;
    case PaymentMethod.CASH:
        // Record cash payment
        break;
}
```

**Step 3: Payment Gateway Integration**
- **VnPay**: Creates payment URL and redirects user
- **MoMo**: Creates payment URL and redirects user
- **Cash**: Records payment in database

**Step 4: Callback Processing**
```csharp
// Payment callbacks update payment status and invoice information
[HttpGet("PaymentCallbackVnpay")]
public async Task<IActionResult> PaymentCallbackVnpay()
{
    // Updates payment status and applies changes to invoice data
}

[HttpGet("PaymentCallBack")]
public async Task<IActionResult> PaymentCallBack()
{
    // Updates payment status and applies changes to invoice data
}
```

### **Database Integration**

#### **Payment Records**
```sql
-- Payments table tracks all payment attempts
CREATE TABLE Payments (
    Id UNIQUEIDENTIFIER PRIMARY KEY,
    InvoiceId UNIQUEIDENTIFIER NOT NULL,
    PaymentDate DATETIME2 NOT NULL,
    Amount DECIMAL(18, 2) NOT NULL,
    PaymentMethod NVARCHAR(50) NOT NULL,
    Status NVARCHAR(50) NOT NULL, -- Pending, Success, Failed, Unpaid
    TransactionCode NVARCHAR(255) NULL
);
```

#### **Invoice Status Updates**
- **Invoice Integration**: Payment information is automatically applied to invoice data when payments are processed
- **Status Updates**: Invoice status is updated to "Paid" when payment succeeds
- **Data Synchronization**: Payment success triggers invoice data changes
- **Tracking**: Complete payment history maintained and linked to invoices

### **Payment Status Flow**

#### **Online Payments (VnPay/MoMo)**
1. **Initial**: Payment created with "Pending" status
2. **Processing**: User redirected to payment gateway
3. **Callback**: Payment status updated to "Success" or "Failed"
4. **Invoice**: Invoice status updated to "Paid" on success

#### **Cash Payments**
1. **Initial**: Payment created with "Unpaid" status
2. **Manual Confirmation**: Admin confirms payment via `MarkPaymentSuccessful`
3. **Final**: Payment status updated to "Success", invoice to "Paid"

## API Endpoints

### **Main Payment Endpoint**
- **URL**: `POST /api/payment/create`
- **Purpose**: Process payment requests and apply payment information to invoice data (CreateUnifiedPayment function)
- **Parameters**: 
  - `request` (UnifiedPaymentRequest): Payment details
  - `paymentMethod` (PaymentMethod): VNPAY, MOMO, or CASH

### **Payment Callbacks**
- **VnPay**: `GET /api/payment/PaymentCallbackVnpay`
- **MoMo**: `GET /api/payment/PaymentCallBack`
- **Purpose**: Handle payment gateway callbacks

### **Manual Payment Confirmation**
- **URL**: `POST /api/payment/mark-payment-successful`
- **Purpose**: Manually confirm cash payments
- **Parameters**: PaymentId, TransactionCode

## Frontend Integration

### **Payment Page (Pay.cshtml)**
```javascript
// Sends payment request to CreateUnifiedPayment function
async function createUnifiedPayment(paymentMethod) {
    const response = await fetch(apiUrl, {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json',
            'Authorization': 'Bearer ' + token
        },
        body: JSON.stringify(paymentRequest)
    });
}
```

### **Payment Request Structure**
```javascript
const paymentRequest = {
    orderId: invoiceId,
    amount: totalAmount,
    orderDescription: 'Utility bill payment',
    name: userName,
    orderType: 'utility_bill'
};
```

## Configuration

### **Payment Gateway Settings**
```json
{
  "Vnpay": {
    "BaseUrl": "https://sandbox.vnpayment.vn/paymentv2/vpcpay.html",
    "TmnCode": "YOUR_TMN_CODE",
    "HashSecret": "YOUR_HASH_SECRET",
    "PaymentBackReturnUrl": "https://your-domain.com/api/payment/PaymentCallbackVnpay"
  },
  "Momo": {
    "PartnerCode": "YOUR_PARTNER_CODE",
    "AccessKey": "YOUR_ACCESS_KEY",
    "SecretKey": "YOUR_SECRET_KEY",
    "ReturnUrl": "https://your-domain.com/api/payment/PaymentCallBack"
  }
}
```

## Error Handling

### **Payment Failures**
- **Automatic Notification**: Failed payments trigger notifications
- **Status Tracking**: Failed payments are logged with error details
- **User Feedback**: Clear error messages returned to frontend

### **Exception Handling**
```csharp
try
{
    // Payment processing logic
}
catch (Exception ex)
{
    // Send failure notification
    var failedNotification = new PushNotificationDto
    {
        Title = "Payment Failed",
        Body = $"Payment for {paymentMethod} failed: {ex.Message}",
        Tag = "payment-failed"
    };
    
    await _pushNotificationService.SendNotificationAsync(failedNotification);
}
```

## Security Considerations

1. **JWT Authentication**: All payment requests require valid JWT tokens
2. **HTTPS Required**: All payment communications use HTTPS
3. **Input Validation**: Payment requests are validated before processing
4. **Transaction Logging**: All payment attempts are logged for audit

## Testing

### **Test Payment Methods**
- **VnPay Sandbox**: Use sandbox environment for testing
- **MoMo Test**: Use test credentials for development
- **Cash Payment**: Test manual confirmation flow

### **Debug Information**
- Payment requests are logged with detailed information
- Callback processing includes comprehensive logging
- Error scenarios are captured and logged

## Troubleshooting

### **Common Issues**
1. **Payment not processing**: Check JWT token and API connectivity
2. **Callback failures**: Verify callback URLs and gateway configuration
3. **Status not updating**: Check database connectivity and transaction handling

### **Debug Steps**
1. Check API logs for payment processing errors
2. Verify payment gateway configuration
3. Monitor database for payment record creation
4. Test callback endpoints manually

## Integration Points

### **Invoice System**
- Payment information automatically applied to invoice data when processed
- Invoice status updates on payment success
- Payment history linked to invoices
- Invoice PDF generation for paid invoices

### **Notification System**
- Payment success/failure notifications
- Real-time status updates to users
- Admin notifications for manual confirmations

### **Reporting System**
- Payment analytics and reporting
- Revenue tracking and analysis
- Payment method usage statistics

## Future Enhancements

- [ ] Additional payment gateways (Stripe, PayPal)
- [ ] Recurring payment support
- [ ] Payment plan management
- [ ] Advanced fraud detection
- [ ] Payment analytics dashboard
- [ ] Mobile payment integration

## Support

For payment system issues:
1. Check API logs for detailed error information
2. Verify payment gateway configuration
3. Test payment flow in sandbox environment
4. Monitor database for payment record integrity 