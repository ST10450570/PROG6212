using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Contract_Monthly_Claim_System.Models
{
    public class Claim
    {
        public int ClaimId { get; set; }

        [Required]
        [StringLength(50)]
        [Display(Name = "Claim Number")]
        public string ClaimNumber { get; set; } = GenerateClaimNumber();

        [Required]
        [DataType(DataType.Date)]
        [Display(Name = "Claim Date")]
        public DateTime ClaimDate { get; set; } = DateTime.UtcNow;

        [Required]
        [DataType(DataType.Date)]
        [Display(Name = "Period Start")]
        public DateTime PeriodStart { get; set; }

        [Required]
        [DataType(DataType.Date)]
        [Display(Name = "Period End")]
        public DateTime PeriodEnd { get; set; }

        [Required]
        [Range(0.1, 200, ErrorMessage = "Hours worked must be between 0.1 and 200")]
        [Display(Name = "Hours Worked")]
        public decimal HoursWorked { get; set; }

        [Required]
        [Range(100, 1000, ErrorMessage = "Hourly rate must be between R100 and R1000")]
        [Display(Name = "Hourly Rate")]
        public decimal HourlyRate { get; set; }

        [Required]
        [Range(0, double.MaxValue)]
        [Display(Name = "Total Amount")]
        public decimal TotalAmount { get; set; }

        [Required]
        [StringLength(20)]
        public string Status { get; set; } = "Draft";

        [Required]
        [StringLength(500)]
        [Display(Name = "Work Description")]
        public string WorkDescription { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Notes { get; set; }

        [StringLength(500)]
        [Display(Name = "Rejection Reason")]
        public string? RejectionReason { get; set; }

        public DateTime? SubmittedDate { get; set; }
        public DateTime? ApprovedDate { get; set; }
        public DateTime? PaidDate { get; set; }

        // Foreign keys
        public string UserId { get; set; } = string.Empty;
        public int? DepartmentId { get; set; }

        // Navigation properties
        public virtual ApplicationUser? User { get; set; }
        public virtual Department? Department { get; set; }
        public virtual ICollection<Document> Documents { get; set; } = new List<Document>();
        public virtual ICollection<ApprovalLog> ApprovalLogs { get; set; } = new List<ApprovalLog>();
        public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();

        // Methods
        public void CalculateTotal()
        {
            TotalAmount = HoursWorked * HourlyRate;
        }

        public void Submit()
        {
            if (!IsValidForSubmission())
                throw new InvalidOperationException("Claim is not valid for submission");

            CalculateTotal();
            Status = "Submitted";
            SubmittedDate = DateTime.UtcNow;
        }

        public void Approve(string approvedBy)
        {
            Status = "Approved";
            ApprovedDate = DateTime.UtcNow;
        }

        public void Reject(string rejectedBy, string reason)
        {
            Status = "Rejected";
            RejectionReason = reason;
        }

        public void ReturnForCorrection()
        {
            Status = "Returned";
        }

        private static string GenerateClaimNumber()
        {
            return $"CLM-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString("N")[..8].ToUpper()}";
        }

        public bool IsValidForSubmission()
        {
            return HoursWorked > 0 &&
                   HourlyRate > 0 &&
                   PeriodStart < PeriodEnd &&
                   !string.IsNullOrWhiteSpace(WorkDescription) &&
                   PeriodEnd <= DateTime.UtcNow;
        }

        // Computed properties
        [Display(Name = "Claim Period")]
        public string ClaimPeriod => $"{PeriodStart:dd MMM yyyy} - {PeriodEnd:dd MMM yyyy}";

        public bool CanEdit => Status == "Draft" || Status == "Returned";
        public bool CanSubmit => Status == "Draft" || Status == "Returned";
        public bool CanApprove => Status == "Submitted" || Status == "UnderReview";
        public bool CanReject => Status == "Submitted" || Status == "UnderReview";
    }
}