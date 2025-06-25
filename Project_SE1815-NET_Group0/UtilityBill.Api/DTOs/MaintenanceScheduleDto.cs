namespace UtilityBill.Api.DTOs
{
    public class MaintenanceScheduleDto
    {
        public int Id { get; set; }
        public int RoomId { get; set; }
        public string? Block { get; set; }
        public string? Title { get; set; }
        public string? Description { get; set; }
        public DateTime ScheduledStart { get; set; }
        public DateTime ScheduledEnd { get; set; }
        public string? Status { get; set; } // e.g., "Scheduled", "In Progress", "Completed"
        public int CreatedByUserId { get; set; } // ID of the user who created the schedule
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
