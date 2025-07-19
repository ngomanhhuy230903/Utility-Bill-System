# 🚀 Quick Start Guide - Push Notifications

## Getting Started in 5 Minutes

### 1. **Database Setup** (1 minute)
Run the database script to create the PushSubscriptions table:
```sql
-- Execute this in SQL Server Management Studio or your database tool
-- File: UtilityBillDb_PushNotifications.sql
```

### 2. **Start the Applications** (2 minutes)
```bash
# Start the API (Terminal 1)
cd Project_SE1815-NET_Group0/UtilityBill.Api
dotnet run

# Start the WebApp (Terminal 2)  
cd Project_SE1815-NET_Group0/UtilityBill.WebApp
dotnet run
```

### 3. **Access the Application** (1 minute)
- **API:** https://localhost:7240
- **WebApp:** https://localhost:7082
- **Home Page:** https://localhost:7082 (Push Notifications Test Page)

### 4. **Test Push Notifications** (1 minute)
1. **Enable Notifications:** Click the "Enable Notifications" button in the navigation bar
2. **Grant Permission:** Allow notifications when prompted by your browser
3. **Test Immediately:** Use the "Send Now" button on the home page
4. **Auto-Test:** Click "Start Auto-Test" to see notifications every 30 seconds

## 🎯 What You'll See

### Home Page Features:
- ✅ **Status Dashboard** - Shows Service Worker, Push Manager, Subscription, and Permission status
- ✅ **Test Buttons** - Send different types of notifications (Test, Invoice, Maintenance, Payment)
- ✅ **Quick Test Controls** - Immediate test and auto-test every 30 seconds
- ✅ **Notification Log** - Real-time log of all notification events
- ✅ **Visual Feedback** - Color-coded status badges and buttons

### Navigation Features:
- 🔔 **Enable Notifications** - Subscribe to push notifications
- 📧 **Send Notifications** - Admin interface for sending notifications
- 🏠 **Home** - Return to the test page

## 🧪 Testing Scenarios

### Scenario 1: Basic Notification Test
1. Enable notifications
2. Click "Send Test Notification"
3. See notification appear (even with browser closed!)

### Scenario 2: Auto-Test Demo
1. Enable notifications
2. Click "Start Auto-Test"
3. Watch notifications appear every 30 seconds
4. Click "Stop Auto-Test" when done

### Scenario 3: Different Notification Types
1. Enable notifications
2. Test each button:
   - **Invoice Notification** - New bill available
   - **Maintenance Notification** - Scheduled maintenance
   - **Payment Reminder** - Payment due soon

### Scenario 4: Admin Interface
1. Navigate to "Send Notifications" in the menu
2. Use templates or create custom notifications
3. Send to all users or specific users

## 🔧 Troubleshooting

### "Service Worker or Push is not supported"
- ✅ Ensure you're using HTTPS (https://localhost:7082)
- ✅ Use a modern browser (Chrome, Firefox, Edge)
- ✅ Check browser console for errors

### "Failed to subscribe"
- ✅ Check that API is running (https://localhost:7240)
- ✅ Verify VAPID keys in appsettings.json
- ✅ Check browser console for network errors

### "Notifications not appearing"
- ✅ Grant notification permissions in browser
- ✅ Check browser notification settings
- ✅ Verify subscription status on the home page

## 📱 Browser Compatibility

| Browser | Service Worker | Push API | Status |
|---------|----------------|----------|---------|
| Chrome | ✅ | ✅ | Full Support |
| Firefox | ✅ | ✅ | Full Support |
| Edge | ✅ | ✅ | Full Support |
| Safari | ✅ | ❌ | Limited (No Push) |

## 🎉 Success Indicators

You'll know everything is working when:
- ✅ All status badges show green (Supported/Subscribed/Granted)
- ✅ Test buttons are enabled (not grayed out)
- ✅ Notifications appear when you click test buttons
- ✅ Notifications work even when browser is closed
- ✅ Auto-test sends notifications every 30 seconds

## 🚀 Next Steps

After testing:
1. **Generate Production VAPID Keys** for deployment
2. **Customize Notification Templates** in the admin interface
3. **Integrate with Business Logic** (invoices, maintenance, payments)
4. **Monitor Performance** using the notification statistics

## 📞 Support

If you encounter issues:
1. Check the browser console (F12) for errors
2. Verify all services are running
3. Check the notification log on the home page
4. Review the detailed README: `PUSH_NOTIFICATIONS_README.md`

---

**🎯 Goal:** Get push notifications working in under 5 minutes!

**✅ Success:** You can send and receive notifications from the home page. 