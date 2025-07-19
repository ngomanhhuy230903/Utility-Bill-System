using UtilityBill.Data.DTOs;

namespace UtilityBill.Business.Interfaces
{
    public interface IPushNotificationService
    {
        Task<NotificationResponseDto> SendNotificationAsync(PushNotificationDto notification);
        Task<NotificationResponseDto> SendNotificationToUserAsync(string userId, PushNotificationDto notification);
        Task<NotificationResponseDto> SendNotificationToAllUsersAsync(PushNotificationDto notification);
    }
} 