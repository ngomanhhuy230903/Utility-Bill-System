// Quick test script for push notifications
class QuickNotificationTest {
    constructor() {
        this.testCount = 0;
        this.autoTestInterval = null;
    }

    // Auto-test notifications every 30 seconds for demo purposes
    startAutoTest() {
        if (this.autoTestInterval) {
            clearInterval(this.autoTestInterval);
        }

        this.autoTestInterval = setInterval(() => {
            this.sendQuickTest();
        }, 30000); // 30 seconds

        console.log('Auto-test started - notifications will be sent every 30 seconds');
    }

    stopAutoTest() {
        if (this.autoTestInterval) {
            clearInterval(this.autoTestInterval);
            this.autoTestInterval = null;
            console.log('Auto-test stopped');
        }
    }

    async sendQuickTest() {
        this.testCount++;
        const messages = [
            { title: 'Test Notification', body: `This is test notification #${this.testCount}` },
            { title: 'System Update', body: 'Your utility bill system is running smoothly!' },
            { title: 'Reminder', body: 'Don\'t forget to check your notifications regularly.' },
            { title: 'Welcome', body: 'Thank you for testing our push notification system!' }
        ];

        const message = messages[this.testCount % messages.length];
        
        try {
            // Always use browser notification by default for reliability
            this.sendBrowserNotification(message.title, message.body);
        } catch (error) {
            console.error('Quick test failed:', error);
        }
    }

    async sendPushNotification(title, body) {
        try {
            const response = await fetch(`${window.API_BASE_URL}/PushNotification/send`, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json'
                },
                body: JSON.stringify({
                    title: title,
                    body: body,
                    tag: 'quick-test',
                    data: JSON.stringify({ testCount: this.testCount })
                })
            });

            if (response.ok) {
                console.log(`Push notification sent: ${title}`);
            } else {
                throw new Error(`API Error: ${response.status}`);
            }
        } catch (error) {
            console.error('Push notification failed:', error);
            throw error;
        }
    }

    sendBrowserNotification(title, body) {
        if ('Notification' in window && Notification.permission === 'granted') {
            const notification = new Notification(title, {
                body: body,
                icon: '/favicon.ico',
                badge: '/favicon.ico',
                tag: 'quick-test',
                requireInteraction: false
            });

            notification.onclick = function() {
                window.focus();
                notification.close();
            };

            console.log(`Browser notification sent: ${title}`);
        } else {
            console.log('Browser notifications not available');
        }
    }

    // Send immediate test notification
    sendImmediateTest() {
        const title = 'Immediate Test';
        const body = 'This is an immediate test notification!';
        
        // Always use browser notification for reliability
        this.sendBrowserNotification(title, body);
    }
}

// Initialize quick test when DOM is loaded
document.addEventListener('DOMContentLoaded', function() {
    window.quickTest = new QuickNotificationTest();
    
    // Only add quick test controls if we're on the notification test page
    if (window.location.pathname === '/NotificationTest') {
        // Add quick test button to the page
        const testContainer = document.querySelector('.card-body');
        if (testContainer) {
            const quickTestDiv = document.createElement('div');
            quickTestDiv.className = 'mt-3 p-3 border rounded bg-light';
            quickTestDiv.innerHTML = `
                <h6><i class="bi bi-lightning"></i> Quick Test Controls</h6>
                <div class="btn-group" role="group">
                    <button type="button" class="btn btn-sm btn-outline-primary" onclick="window.quickTest.sendImmediateTest()">
                        <i class="bi bi-play"></i> Send Now
                    </button>
                    <button type="button" class="btn btn-sm btn-outline-success" onclick="window.quickTest.startAutoTest()">
                        <i class="bi bi-arrow-repeat"></i> Start Auto-Test
                    </button>
                    <button type="button" class="btn btn-sm btn-outline-danger" onclick="window.quickTest.stopAutoTest()">
                        <i class="bi bi-stop"></i> Stop Auto-Test
                    </button>
                    <button type="button" class="btn btn-sm btn-outline-warning" onclick="window.pushNotificationManager.forceRefreshSubscription()">
                        <i class="bi bi-arrow-clockwise"></i> Refresh Subscription
                    </button>
                    <button type="button" class="btn btn-sm btn-outline-info" onclick="window.pushNotificationManager.sendNotificationDirectly('Direct Test', 'This is a direct browser notification test!')">
                        <i class="bi bi-bell"></i> Direct Test
                    </button>
                </div>
                <small class="text-muted d-block mt-2">Auto-test sends notifications every 30 seconds for demo purposes.</small>
            `;
            
            testContainer.appendChild(quickTestDiv);
        }
    }
}); 