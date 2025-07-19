using System.ComponentModel.DataAnnotations;

namespace UtilityBill.Data.Models
{
    public class PushSubscription
    {
        public int Id { get; set; }
        
        [MaxLength(450)]
        public string? UserId { get; set; }
        
        [Required]
        public string Endpoint { get; set; } = null!;
        
        [Required]
        public string P256Dh { get; set; } = null!;
        
        [Required]
        public string Auth { get; set; } = null!;
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        public bool IsActive { get; set; } = true;
        
        public virtual User? User { get; set; }
    }
} 