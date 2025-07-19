using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using UtilityBill.Business.Interfaces;
using UtilityBill.Data.Context;
using UtilityBill.Data.DTOs;
using UtilityBill.Data.Models;
using UtilityBill.Data.Repositories;

namespace UtilityBill.Business.Services
{
    public class PushNotificationService : IPushNotificationService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IConfiguration _configuration;
        private readonly ILogger<PushNotificationService> _logger;
        private readonly HttpClient _httpClient;
        private readonly string _vapidPublicKey;
        private readonly string _vapidPrivateKey;
        private readonly string _vapidSubject;

        public PushNotificationService(
            IUnitOfWork unitOfWork,
            IConfiguration configuration,
            ILogger<PushNotificationService> logger,
            HttpClient httpClient)
        {
            _unitOfWork = unitOfWork;
            _configuration = configuration;
            _logger = logger;
            _httpClient = httpClient;
            
            _vapidPublicKey = _configuration["Vapid:PublicKey"] ?? throw new InvalidOperationException("Vapid:PublicKey not configured");
            _vapidPrivateKey = _configuration["Vapid:PrivateKey"] ?? throw new InvalidOperationException("Vapid:PrivateKey not configured");
            _vapidSubject = _configuration["Vapid:Subject"] ?? "mailto:admin@utilitybill.com";
        }

        public async Task<NotificationResponseDto> SendNotificationAsync(PushNotificationDto notification)
        {
            if (notification.UserIds?.Any() == true)
            {
                return await SendNotificationToSpecificUsersAsync(notification);
            }
            else
            {
                return await SendNotificationToAllUsersAsync(notification);
            }
        }

        public async Task<NotificationResponseDto> SendNotificationToUserAsync(string userId, PushNotificationDto notification)
        {
            var response = new NotificationResponseDto();
            
            try
            {
                var subscriptions = await _unitOfWork.PushSubscriptionRepository.GetActiveSubscriptionsByUserIdAsync(userId);
                
                foreach (var subscription in subscriptions)
                {
                    var success = await SendNotificationToSubscriptionAsync(subscription, notification);
                    if (success)
                        response.SentCount++;
                    else
                        response.FailedCount++;
                }

                response.Success = response.FailedCount == 0;
                response.Message = $"Sent {response.SentCount} notifications, {response.FailedCount} failed";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending notification to user {UserId}", userId);
                response.Success = false;
                response.Message = ex.Message;
            }

            return response;
        }

        public async Task<NotificationResponseDto> SendNotificationToAllUsersAsync(PushNotificationDto notification)
        {
            var response = new NotificationResponseDto();
            
            try
            {
                var subscriptions = await _unitOfWork.PushSubscriptionRepository.GetAllActiveSubscriptionsAsync();
                
                foreach (var subscription in subscriptions)
                {
                    var success = await SendNotificationToSubscriptionAsync(subscription, notification);
                    if (success)
                        response.SentCount++;
                    else
                        response.FailedCount++;
                }

                response.Success = response.FailedCount == 0;
                response.Message = $"Sent {response.SentCount} notifications, {response.FailedCount} failed";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending notification to all users");
                response.Success = false;
                response.Message = ex.Message;
            }

            return response;
        }

        private async Task<NotificationResponseDto> SendNotificationToSpecificUsersAsync(PushNotificationDto notification)
        {
            var response = new NotificationResponseDto();
            
            try
            {
                foreach (var userId in notification.UserIds!)
                {
                    var userResponse = await SendNotificationToUserAsync(userId, notification);
                    response.SentCount += userResponse.SentCount;
                    response.FailedCount += userResponse.FailedCount;
                }

                response.Success = response.FailedCount == 0;
                response.Message = $"Sent {response.SentCount} notifications, {response.FailedCount} failed";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending notification to specific users");
                response.Success = false;
                response.Message = ex.Message;
            }

            return response;
        }

        private async Task<bool> SendNotificationToSubscriptionAsync(PushSubscription subscription, PushNotificationDto notification)
        {
            try
            {
                var payload = CreateNotificationPayload(notification);
                var encryptedPayload = EncryptPayload(payload, subscription.P256Dh, subscription.Auth);
                
                var request = new HttpRequestMessage(HttpMethod.Post, subscription.Endpoint);
                
                // Add VAPID headers
                var vapidHeaders = CreateVapidHeaders(subscription.Endpoint);
                request.Headers.Authorization = new AuthenticationHeaderValue("vapid", vapidHeaders.Authorization);
                request.Headers.Add("Crypto-Key", vapidHeaders.CryptoKey);
                
                // Add encryption header to request headers
                request.Headers.Add("Encryption", $"salt={Convert.ToBase64String(encryptedPayload.Salt)}");
                
                request.Content = new ByteArrayContent(encryptedPayload.Payload);
                request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
                // Add Content-Encoding to content headers, not request headers
                request.Content.Headers.Add("Content-Encoding", "aes128gcm");

                var response = await _httpClient.SendAsync(request);
                
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("Failed to send notification to {Endpoint}. Status: {StatusCode}", 
                        subscription.Endpoint, response.StatusCode);
                    
                    // If subscription is invalid, deactivate it
                    if (response.StatusCode == System.Net.HttpStatusCode.Gone || 
                        response.StatusCode == System.Net.HttpStatusCode.NotFound)
                    {
                        await _unitOfWork.PushSubscriptionRepository.DeactivateSubscriptionAsync(subscription.Endpoint);
                        await _unitOfWork.SaveChangesAsync();
                    }
                    
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending notification to subscription {Endpoint}", subscription.Endpoint);
                return false;
            }
        }

        private string CreateNotificationPayload(PushNotificationDto notification)
        {
            var payload = new
            {
                title = notification.Title,
                body = notification.Body,
                icon = notification.Icon ?? "/favicon.ico",
                badge = notification.Badge ?? "/favicon.ico",
                tag = notification.Tag,
                data = notification.Data,
                timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
            };

            return JsonSerializer.Serialize(payload);
        }

        private (byte[] Payload, byte[] Salt) EncryptPayload(string payload, string p256dh, string auth)
        {
            var salt = new byte[16];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(salt);
            }

            // This is a simplified encryption. In production, you should use a proper Web Push encryption library
            // For now, we'll return the payload as-is with a salt
            var payloadBytes = Encoding.UTF8.GetBytes(payload);
            
            return (payloadBytes, salt);
        }

        private (string Authorization, string CryptoKey) CreateVapidHeaders(string endpoint)
        {
            // This is a simplified VAPID header creation
            // In production, you should use a proper VAPID library
            var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            var token = $"{_vapidSubject}\n{timestamp}";
            
            // For now, return placeholder headers
            return ($"t={timestamp},k={_vapidPublicKey}", $"p256ecdsa={_vapidPublicKey}");
        }
    }
} 