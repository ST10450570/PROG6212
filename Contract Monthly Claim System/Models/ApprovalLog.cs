using System.ComponentModel.DataAnnotations;

namespace Contract_Monthly_Claim_System.Models
{
    public class ApprovalLog
    {
        public int ApprovalLogId { get; set; }

        [Required]
        public DateTime ActionDate { get; set; } = DateTime.Now;

        [Required]
        [StringLength(20)]
        public string Action { get; set; } = string.Empty; // Approved, Rejected

        [StringLength(500)]
        public string? Comments { get; set; }

        // Foreign keys
        public int ClaimId { get; set; }
        public string ApproverId { get; set; } = string.Empty;

        // Navigation properties
        public virtual Claim? Claim { get; set; }
        public virtual ApplicationUser? Approver { get; set; }

        public void LogAction(string action, string? comments = null)
        {
            Action = action;
            Comments = comments;
            ActionDate = DateTime.Now;
        }
    }
}