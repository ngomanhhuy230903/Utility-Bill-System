// File: UtilityBill.Business/DTOs/TenantHistoryDto.cs
using System;

namespace UtilityBill.Shared.DTOs
{
    // DTO này là một phiên bản "phẳng" của TenantHistory để trả về cho client
    public class TenantHistoryDto
    {
        public int Id { get; set; }
        public int RoomId { get; set; }
        public string RoomNumber { get; set; }
        public string TenantId { get; set; }
        public string TenantName { get; set; }
        public DateOnly MoveInDate { get; set; }
        public DateOnly? MoveOutDate { get; set; }
    }
}