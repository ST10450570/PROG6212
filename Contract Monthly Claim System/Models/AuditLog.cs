using System.ComponentModel.DataAnnotations;

namespace Contract_Monthly_Claim_System.Models
{
    public class AuditLog
    {
        public int AuditLogId { get; set; }

        [Required]
        public DateTime Timestamp { get; set; } = DateTime.Now;

        [Required]
        [StringLength(450)] // Changed from 100 to 450 to match Identity User Id
        public string UserId { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string TableName { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string Action { get; set; } = string.Empty; // CREATE, UPDATE, DELETE

        public string? RecordId { get; set; }
        public string? OldValues { get; set; }
        public string? NewValues { get; set; }

        [StringLength(200)]
        public string? IpAddress { get; set; }

        [StringLength(500)]
        public string? UserAgent { get; set; }

        // Navigation property
        public virtual ApplicationUser? User { get; set; }
    }
}