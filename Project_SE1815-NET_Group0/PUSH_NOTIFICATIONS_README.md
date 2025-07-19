# Web Push Notifications Implementation

This document explains how to set up and use the web push notification system in the Utility Bill Management application.

## Overview

The push notification system allows the application to send real-time notifications to users' browsers, even when the application is not actively open. This is useful for:

- Maintenance reminders
- New invoice notifications
- Payment due reminders
- General announcements

## Features

- ✅ User subscription management
- ✅ Admin notification sending interface
- ✅ Background service integration
- ✅ Service worker for offline support
- ✅ VAPID authentication
- ✅ Notification templates
- ✅ User-specific and broadcast notifications

## Setup Instructions

### 1. Database Setup

Run the SQL script to create the PushSubscriptions table:

```sql
-- Execute the UtilityBillDb_PushNotifications.sql script
```

### 2. VAPID Keys Configuration

The application uses VAPID (Voluntary Application Server Identification) for secure push notifications. 

**For Development:**
The current configuration uses sample keys. For production, generate your own VAPID keys:

```bash
# Using web-push library (Node.js)
npm install web-push -g
web-push generate-vapid-keys
```

**Update the keys in `appsettings.json`:**
```json
{
  "Vapid": {
    "PublicKey": "YOUR_PUBLIC_KEY",
    "PrivateKey": "YOUR_PRIVATE_KEY",
    "Subject": "mailto:your-email@domain.com"
  }
}
```

### 3. HTTPS Requirement

Push notifications require HTTPS. For development:
- Use `https://localhost:7082` for the web app
- Use `https://localhost:7240` for the API

### 4. Browser Permissions

Users need to grant notification permissions in their browser:
1. Click "Enable Notifications" button
2. Allow notifications when prompted
3. The subscription will be saved to the database

## Usage

### For Users

1. **Enable Notifications:**
   - Click the "Enable Notifications" button in the navigation bar
   - Grant permission when prompted by the browser
   - The button will change to "Disable Notifications" when active

2. **Receive Notifications:**
   - Notifications will appear even when the browser is closed
   - Click on notifications to open the application
   - Use notification actions (View/Close)

### For Administrators

1. **Send Notifications:**
   - Navigate to `/Admin/Notifications`
   - Fill in the notification details
   - Choose to send to all users or specific users
   - Use templates for common notifications

2. **Available Templates:**
   - **New Invoice Available:** Notify users about new bills
   - **Maintenance Reminder:** Alert about scheduled maintenance
   - **Payment Due Reminder:** Remind about upcoming payments
   - **General Announcement:** Send general messages

3. **API Endpoints:**
   - `POST /api/PushNotification/subscribe` - Subscribe user
   - `POST /api/PushNotification/unsubscribe` - Unsubscribe user
   - `POST /api/PushNotification/send` - Send notification
   - `GET /api/PushNotification/subscriptions` - Get all subscriptions

## Technical Implementation

### Backend Components

1. **Models:**
   - `PushSubscription` - Database model for subscriptions
   - `PushSubscriptionDto` - API DTOs

2. **Services:**
   - `PushNotificationService` - Core notification logic
   - `IPushNotificationService` - Service interface

3. **Controllers:**
   - `PushNotificationController` - API endpoints

4. **Background Services:**
   - `MaintenanceReminderJob` - Automated maintenance notifications

### Frontend Components

1. **Service Worker (`sw.js`):**
   - Handles push events
   - Manages notification display
   - Provides offline caching

2. **JavaScript (`push-notifications.js`):**
   - Manages user subscriptions
   - Handles permission requests
   - Communicates with API

3. **Admin Interface (`/Admin/Notifications`):**
   - Send notifications
   - View statistics
   - Use templates

## Security Considerations

1. **VAPID Authentication:**
   - Ensures only your server can send notifications
   - Prevents unauthorized push messages

2. **User Authorization:**
   - Only authenticated users can subscribe
   - Only admins can send notifications

3. **HTTPS Requirement:**
   - Push notifications only work over HTTPS
   - Ensures secure communication

## Troubleshooting

### Common Issues

1. **"Service Worker or Push is not supported"**
   - Ensure HTTPS is enabled
   - Check browser compatibility (Chrome, Firefox, Edge)

2. **"Failed to subscribe"**
   - Check VAPID keys configuration
   - Verify API endpoints are accessible
   - Check browser console for errors

3. **"Notifications not appearing"**
   - Check browser notification permissions
   - Verify service worker is registered
   - Check network connectivity

### Debug Steps

1. **Check Browser Console:**
   ```javascript
   // Check service worker registration
   navigator.serviceWorker.getRegistrations()
   
   // Check push manager
   navigator.serviceWorker.ready.then(registration => {
     registration.pushManager.getSubscription()
   })
   ```

2. **Check API Responses:**
   - Monitor network tab in browser dev tools
   - Check API logs for errors

3. **Verify Database:**
   ```sql
   SELECT * FROM PushSubscriptions WHERE IsActive = 1
   ```

## Production Deployment

1. **Generate Production VAPID Keys:**
   ```bash
   web-push generate-vapid-keys --json
   ```

2. **Update Configuration:**
   - Set production VAPID keys
   - Update subject email
   - Configure HTTPS certificates

3. **Monitor Performance:**
   - Track notification delivery rates
   - Monitor subscription counts
   - Check for failed deliveries

4. **Security:**
   - Use strong VAPID keys
   - Implement rate limiting
   - Monitor for abuse

## API Reference

### Subscribe User
```http
POST /api/PushNotification/subscribe
Content-Type: application/json
Authorization: Bearer {token}

{
  "endpoint": "https://fcm.googleapis.com/fcm/send/...",
  "p256dh": "base64-encoded-p256dh-key",
  "auth": "base64-encoded-auth-key"
}
```

### Send Notification
```http
POST /api/PushNotification/send
Content-Type: application/json
Authorization: Bearer {admin-token}

{
  "title": "Notification Title",
  "body": "Notification message",
  "icon": "https://example.com/icon.png",
  "tag": "notification-tag",
  "data": "{\"url\": \"/some-page\"}",
  "userIds": ["user1", "user2"] // Optional, null for all users
}
```

## Support

For issues or questions:
1. Check browser console for errors
2. Verify configuration settings
3. Test with different browsers
4. Check API logs for backend errors

## Future Enhancements

- [ ] Notification history tracking
- [ ] Notification preferences per user
- [ ] Rich media notifications
- [ ] Notification analytics
- [ ] Mobile app integration
- [ ] Email fallback for failed push notifications 