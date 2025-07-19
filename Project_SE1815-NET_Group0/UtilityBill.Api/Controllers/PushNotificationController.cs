using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using UtilityBill.Business.Interfaces;
using UtilityBill.Data.Models;
using UtilityBill.Data.Repositories;
using UtilityBill.Data.DTOs;

namespace UtilityBill.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PushNotificationController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IPushNotificationService _pushNotificationService;
        private readonly ILogger<PushNotificationController> _logger;

        public PushNotificationController(
            IUnitOfWork unitOfWork,
            IPushNotificationService pushNotificationService,
            ILogger<PushNotificationController> logger)
        {
            _unitOfWork = unitOfWork;
            _pushNotificationService = pushNotificationService;
            _logger = logger;
        }

        [HttpPost("subscribe")]
        [AllowAnonymous]
        public async Task<IActionResult> Subscribe([FromBody] PushSubscriptionDto subscriptionDto)
        {
            try
            {
                // Get user ID if authenticated, otherwise null for anonymous users
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                // Check if subscription already exists
                var existingSubscription = await _unitOfWork.PushSubscriptionRepository.GetByEndpointAsync(subscriptionDto.Endpoint);
                if (existingSubscription != null)
                {
                    // Update existing subscription
                    existingSubscription.UserId = userId; // Update user ID in case user logged in
                    existingSubscription.P256Dh = subscriptionDto.P256Dh;
                    existingSubscription.Auth = subscriptionDto.Auth;
                    existingSubscription.IsActive = true;
                    _unitOfWork.PushSubscriptionRepository.Update(existingSubscription);
                }
                else
                {
                    // Create new subscription
                    var subscription = new PushSubscription
                    {
                        UserId = userId, // null for anonymous users
                        Endpoint = subscriptionDto.Endpoint,
                        P256Dh = subscriptionDto.P256Dh,
                        Auth = subscriptionDto.Auth,
                        IsActive = true
                    };
                    await _unitOfWork.PushSubscriptionRepository.AddAsync(subscription);
                }

                await _unitOfWork.SaveChangesAsync();
                return Ok(new { message = "Subscription saved successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving push subscription");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        [HttpPost("unsubscribe")]
        [AllowAnonymous]
        public async Task<IActionResult> Unsubscribe([FromBody] PushSubscriptionDto subscriptionDto)
        {
            try
            {
                await _unitOfWork.PushSubscriptionRepository.DeactivateSubscriptionAsync(subscriptionDto.Endpoint);
                await _unitOfWork.SaveChangesAsync();
                return Ok(new { message = "Subscription removed successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing push subscription");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        [HttpPost("send")]
        [AllowAnonymous]
        public async Task<IActionResult> SendNotification([FromBody] PushNotificationDto notificationDto)
        {
            try
            {
                var result = await _pushNotificationService.SendNotificationAsync(notificationDto);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending push notification");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        [HttpPost("send-to-user")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> SendNotificationToUser([FromBody] PushNotificationDto notificationDto, [FromQuery] string userId)
        {
            try
            {
                var result = await _pushNotificationService.SendNotificationToUserAsync(userId, notificationDto);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending push notification to user {UserId}", userId);
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        [HttpGet("subscriptions")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetSubscriptions()
        {
            try
            {
                var subscriptions = await _unitOfWork.PushSubscriptionRepository.GetAllActiveSubscriptionsAsync();
                return Ok(subscriptions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting push subscriptions");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        [HttpGet("my-subscriptions")]
        [Authorize]
        public async Task<IActionResult> GetMySubscriptions()
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized();
                }

                var subscriptions = await _unitOfWork.PushSubscriptionRepository.GetActiveSubscriptionsByUserIdAsync(userId);
                return Ok(subscriptions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user push subscriptions");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }
    }
} 