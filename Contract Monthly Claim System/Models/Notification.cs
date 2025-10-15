using System.ComponentModel.DataAnnotations;

namespace Contract_Monthly_Claim_System.Models
{
    public class Notification
    {
        public int NotificationId { get; set; }

        [Required]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;

        [Required]
        public string Message { get; set; } = string.Empty;

        [Required]
        public string UserId { get; set; } = string.Empty;

        public bool IsRead { get; set; } = false;
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public DateTime? ReadDate { get; set; }

        [StringLength(50)]
        public string Type { get; set; } = "Info"; // Info, Success, Warning, Error

        [StringLength(500)]
        public string? ActionUrl { get; set; }

        // Navigation property
        public virtual ApplicationUser? User { get; set; }
    }
}