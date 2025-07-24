# Notification System Documentation

## Overview

The Utility Bill Management System includes a comprehensive push notification system that automatically sends real-time notifications to users' browsers for various system events. This system uses Web Push API technology to deliver notifications even when the application is not actively open.

## How Notifications Work

### 1. **Service Worker Integration**
- Notifications are handled by a service worker (`sw.js`) that runs in the background
- The service worker receives push events and displays notifications to users
- Users must grant notification permissions in their browser

### 2. **Push Subscription Management**
- Users subscribe to notifications via the `PushNotificationController`
- Subscription data is stored in the `PushSubscriptions` table
- Each subscription includes endpoint, encryption keys, and user association

### 3. **VAPID Authentication**
- Uses VAPID (Voluntary Application Server Identification) for secure push notifications
- Ensures only authorized servers can send notifications to subscribed users
- Configuration required in `appsettings.json`

## Notification Events

The system automatically sends notifications for the following events:

### **Object Creation Events**

#### 1. **Room Creation**
- **Trigger**: When a new room is created via `RoomService.CreateRoomAsync()`
- **Notification**: "New Room Created"
- **Data**: Room ID, room number, block, floor
- **Tag**: `room-created`

#### 2. **User Registration**
- **Trigger**: When a new user registers via `AuthController.Register()`
- **Notification**: "New User Registered"
- **Data**: User ID, username, full name, email
- **Tag**: `user-registered`

#### 3. **Meter Reading Creation**
- **Trigger**: When meter readings are recorded via `MeterReadingService.CreateMeterReadingAsync()`
- **Notification**: "Meter Reading Created"
- **Data**: Room ID, room number, month, year, electric/water readings
- **Tag**: `meter-reading-created`

#### 4. **Maintenance Schedule Creation**
- **Trigger**: When maintenance is scheduled via `MaintenanceScheduleService.Create()`
- **Notification**: "Maintenance Scheduled"
- **Data**: Maintenance ID, title, description, scheduled times, room ID
- **Tag**: `maintenance-scheduled`

### **Payment Events**

#### 5. **Successful Payments**
- **Trigger**: When payments are successfully processed
- **Notifications**:
  - VnPay: "Payment Successful!" (via callback)
  - MoMo: "Payment Successful!" (via callback)
  - Cash: "Payment Confirmed!" (via manual confirmation)
- **Data**: Payment method, amount, transaction details
- **Tags**: `payment-success`, `payment-confirmed`

#### 6. **Failed Payments**
- **Trigger**: When payment processing fails
- **Notification**: "Payment Failed"
- **Data**: Payment method, error details
- **Tag**: `payment-failed`

### **Tenant Management Events**

#### 7. **Tenant Assignment**
- **Trigger**: When a tenant is assigned to a room via `RoomService.AssignTenantAsync()`
- **Notification**: "Tenant Assigned"
- **Data**: Room ID, room number, tenant ID, tenant name, move-in date
- **Tag**: `tenant-assigned`

#### 8. **Tenant Unassignment**
- **Trigger**: When a tenant is removed from a room via `RoomService.UnassignTenantAsync()`
- **Notification**: "Tenant Unassigned"
- **Data**: Room ID, room number, tenant ID, tenant name, move-out date
- **Tag**: `tenant-unassigned`

### **Billing Events**

#### 9. **Invoice Generation**
- **Trigger**: When monthly invoices are automatically generated via `BillingService.GenerateInvoicesForPreviousMonthAsync()`
- **Notification**: "New Invoices Generated"
- **Data**: Month, year, count of invoices generated
- **Tag**: `invoices-generated`

## Technical Implementation

### **Backend Components**

#### **Services with Notification Integration**
```csharp
// All services inject IPushNotificationService
public class RoomService : IRoomService
{
    private readonly IPushNotificationService _pushNotificationService;
    
    // Notification calls in business methods
    await _pushNotificationService.SendNotificationAsync(notification);
}
```

#### **Notification Data Structure**
```csharp
public class PushNotificationDto
{
    public string Title { get; set; }
    public string Body { get; set; }
    public string? Icon { get; set; }
    public string? Badge { get; set; }
    public string? Tag { get; set; }
    public string? Data { get; set; }
    public List<string>? UserIds { get; set; }
}
```

### **Frontend Components**

#### **Service Worker (`sw.js`)**
- Handles push events
- Displays notifications with actions
- Manages notification interactions

#### **JavaScript Integration (`push-notifications.js`)**
- Manages user subscriptions
- Handles permission requests
- Communicates with API endpoints

## Configuration

### **Required Settings in `appsettings.json`**
```json
{
  "Vapid": {
    "PublicKey": "YOUR_PUBLIC_KEY",
    "PrivateKey": "YOUR_PRIVATE_KEY",
    "Subject": "mailto:admin@utilitybill.com"
  }
}
```

### **Database Requirements**
- `PushSubscriptions` table must exist
- User authentication system must be configured
- HTTPS must be enabled for production

## Usage Examples

### **Sending a Notification**
```csharp
var notification = new PushNotificationDto
{
    Title = "Event Title",
    Body = "Event description",
    Tag = "event-tag",
    Data = JsonSerializer.Serialize(new { type = "event_type", id = "123" })
};

await _pushNotificationService.SendNotificationAsync(notification);
```

### **Error Handling**
```csharp
try
{
    await _pushNotificationService.SendNotificationAsync(notification);
}
catch (Exception ex)
{
    // Log error but don't fail the main operation
    _logger.LogError(ex, "Failed to send notification");
}
```

## Security Considerations

1. **VAPID Authentication**: Ensures only authorized servers can send notifications
2. **User Authorization**: Only authenticated users can subscribe to notifications
3. **HTTPS Requirement**: Push notifications only work over secure connections
4. **Data Privacy**: Notification data is encrypted in transit

## Troubleshooting

### **Common Issues**
1. **Notifications not appearing**: Check browser permissions and service worker registration
2. **Subscription failures**: Verify VAPID keys and HTTPS configuration
3. **Delivery failures**: Check network connectivity and endpoint validity

### **Debug Steps**
1. Check browser console for service worker errors
2. Verify API endpoints are accessible
3. Monitor notification delivery in browser dev tools
4. Check database for active subscriptions

## Future Enhancements

- [ ] Notification preferences per user
- [ ] Notification history tracking
- [ ] Rich media notifications
- [ ] Notification analytics
- [ ] Email fallback for failed push notifications
- [ ] Mobile app integration

## API Endpoints

- `POST /api/PushNotification/subscribe` - Subscribe user to notifications
- `POST /api/PushNotification/unsubscribe` - Unsubscribe user from notifications
- `POST /api/PushNotification/send` - Send notification to all users
- `POST /api/PushNotification/send-to-user` - Send notification to specific user

## Support

For issues or questions about the notification system:
1. Check browser console for errors
2. Verify configuration settings
3. Test with different browsers
4. Check API logs for backend errors 