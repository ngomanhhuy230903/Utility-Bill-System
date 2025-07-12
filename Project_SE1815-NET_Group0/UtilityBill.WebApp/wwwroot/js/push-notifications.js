// Push Notification Management
class PushNotificationManager {
    constructor() {
        this.apiBaseUrl = window.API_BASE_URL || 'https://localhost:7240/api';
        this.vapidPublicKey = 'BEl62iUYgUivxIkv69yViEuiBIa1eQeY7HF_xtvsvHjvATNnGozS_6NhW6iUktddqzItp40Ejix5uLrcu61VpJQ';
        this.isSubscribed = false;
        this.swRegistration = null;
    }

    async initialize() {
        try {
            if ('serviceWorker' in navigator && 'PushManager' in window) {
                console.log('Service Worker and Push are supported');
                
                this.swRegistration = await navigator.serviceWorker.register('/sw.js');
                console.log('Service Worker registered:', this.swRegistration);

                // Automatically request notification permission
                await this.requestNotificationPermission();
                
                await this.updateSubscriptionOnServer();
            } else {
                console.log('Service Worker or Push is not supported');
            }
        } catch (error) {
            console.error('Error initializing push notifications:', error);
        }
    }

    async requestNotificationPermission() {
        try {
            if ('Notification' in window) {
                const permission = Notification.permission;
                
                if (permission === 'default') {
                    console.log('Requesting notification permission...');
                    const result = await Notification.requestPermission();
                    
                    if (result === 'granted') {
                        console.log('Notification permission granted');
                        // Send welcome notification
                        this.sendWelcomeNotification();
                    } else {
                        console.log('Notification permission denied');
                    }
                } else if (permission === 'granted') {
                    console.log('Notification permission already granted');
                    // Send welcome notification if not already sent
                    this.sendWelcomeNotification();
                } else {
                    console.log('Notification permission denied');
                }
            }
        } catch (error) {
            console.error('Error requesting notification permission:', error);
        }
    }

    sendWelcomeNotification() {
        try {
            const notification = new Notification('Welcome to Utility Bill Management!', {
                body: 'Thank you for enabling notifications. You\'ll now receive important updates about your utility bills and maintenance schedules.',
                icon: '/favicon.ico',
                badge: '/favicon.ico',
                tag: 'welcome-notification',
                requireInteraction: false
            });

            notification.onclick = function() {
                window.focus();
                notification.close();
            };

            console.log('Welcome notification sent');
        } catch (error) {
            console.error('Error sending welcome notification:', error);
        }
    }

    async updateSubscriptionOnServer() {
        try {
            const subscription = await this.swRegistration.pushManager.getSubscription();
            this.isSubscribed = !(subscription === null);
            
            console.log('Current subscription status:', this.isSubscribed);
            console.log('Subscription object:', subscription);
            
            if (this.isSubscribed) {
                console.log('User is subscribed, updating server...');
                await this.updateSubscriptionOnServer(subscription);
            } else {
                console.log('User is NOT subscribed');
            }
        } catch (error) {
            console.error('Error checking subscription:', error);
        }
    }

    async subscribeUser() {
        try {
            const applicationServerKey = this.urlBase64ToUint8Array(this.vapidPublicKey);
            const subscription = await this.swRegistration.pushManager.subscribe({
                userVisibleOnly: true,
                applicationServerKey: applicationServerKey
            });

            console.log('User is subscribed:', subscription);
            this.isSubscribed = true;
            await this.updateSubscriptionOnServer(subscription);
        } catch (error) {
            console.error('Failed to subscribe the user:', error);
            // Fall back to browser notifications
            this.showNotificationError('Push subscription failed, using browser notifications instead');
            this.isSubscribed = false;
        }
    }

    async unsubscribeUser() {
        try {
            const subscription = await this.swRegistration.pushManager.getSubscription();
            if (subscription) {
                await subscription.unsubscribe();
                console.log('User is unsubscribed');
                this.isSubscribed = false;
                await this.updateSubscriptionOnServer(null);
            }
        } catch (error) {
            console.error('Error unsubscribing:', error);
        }
    }

    async updateSubscriptionOnServer(subscription) {
        try {
            if (subscription) {
                // Subscribe
                console.log('Sending subscription to server:', subscription.endpoint);
                const response = await fetch(`${this.apiBaseUrl}/PushNotification/subscribe`, {
                    method: 'POST',
                    headers: {
                        'Content-Type': 'application/json'
                    },
                    body: JSON.stringify({
                        endpoint: subscription.endpoint,
                        p256dh: this.arrayBufferToBase64(subscription.getKey('p256dh')),
                        auth: this.arrayBufferToBase64(subscription.getKey('auth'))
                    })
                });

                if (response.ok) {
                    console.log('Subscription saved to server successfully');
                } else {
                    const errorText = await response.text();
                    console.error('Failed to save subscription to server:', response.status, errorText);
                }
            } else {
                // Unsubscribe
                console.log('Removing subscription from server');
                const response = await fetch(`${this.apiBaseUrl}/PushNotification/unsubscribe`, {
                    method: 'POST',
                    headers: {
                        'Content-Type': 'application/json'
                    },
                    body: JSON.stringify({
                        endpoint: '',
                        p256dh: '',
                        auth: ''
                    })
                });

                if (response.ok) {
                    console.log('Subscription removed from server successfully');
                } else {
                    const errorText = await response.text();
                    console.error('Failed to remove subscription from server:', response.status, errorText);
                }
            }
        } catch (error) {
            console.error('Error updating subscription on server:', error);
        }
    }

    showNotificationError(message) {
        // You can implement a toast notification here
        console.error(message);
    }

    async forceRefreshSubscription() {
        try {
            console.log('Force refreshing subscription...');
            
            // Unsubscribe first
            const existingSubscription = await this.swRegistration.pushManager.getSubscription();
            if (existingSubscription) {
                await existingSubscription.unsubscribe();
                console.log('Unsubscribed from existing subscription');
            }
            
            // Subscribe again
            await this.subscribeUser();
            console.log('Subscription refreshed successfully');
        } catch (error) {
            console.error('Error refreshing subscription:', error);
        }
    }

    // Add method to send notification directly (bypassing push API)
    async sendNotificationDirectly(title, body, options = {}) {
        try {
            // Always use browser notification for reliability
            return this.sendBrowserNotification(title, body, options);
        } catch (error) {
            console.error('Error sending direct notification:', error);
            return false;
        }
    }

    // Add method to send browser notification directly
    sendBrowserNotification(title, body, options = {}) {
        if ('Notification' in window && Notification.permission === 'granted') {
            const notification = new Notification(title, {
                body: body,
                icon: options.icon || '/favicon.ico',
                badge: options.badge || '/favicon.ico',
                tag: options.tag || 'utility-bill-notification',
                data: options.data || {},
                requireInteraction: options.requireInteraction || false,
                ...options
            });

            notification.onclick = function() {
                window.focus();
                notification.close();
            };

            console.log(`Browser notification sent: ${title}`);
            return true;
        } else {
            console.log('Browser notifications not available or permission denied');
            return false;
        }
    }

    // Utility functions
    urlBase64ToUint8Array(base64String) {
        const padding = '='.repeat((4 - base64String.length % 4) % 4);
        const base64 = (base64String + padding)
            .replace(/-/g, '+')
            .replace(/_/g, '/');

        const rawData = window.atob(base64);
        const outputArray = new Uint8Array(rawData.length);

        for (let i = 0; i < rawData.length; ++i) {
            outputArray[i] = rawData.charCodeAt(i);
        }
        return outputArray;
    }

    arrayBufferToBase64(buffer) {
        const bytes = new Uint8Array(buffer);
        let binary = '';
        for (let i = 0; i < bytes.byteLength; i++) {
            binary += String.fromCharCode(bytes[i]);
        }
        return window.btoa(binary);
    }
}

// Initialize push notification manager when DOM is loaded
document.addEventListener('DOMContentLoaded', () => {
    window.pushNotificationManager = new PushNotificationManager();
    window.pushNotificationManager.initialize();
}); 