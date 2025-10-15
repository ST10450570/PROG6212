using System.ComponentModel.DataAnnotations;

namespace Contract_Monthly_Claim_System.Models
{
    public class Contract
    {
        public int ContractId { get; set; }

        [Required]
        [StringLength(100)]
        public string ContractNumber { get; set; } = string.Empty;

        [Required]
        public string UserId { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.Date)]
        public DateTime StartDate { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime EndDate { get; set; }

        [Required]
        [Range(0.01, double.MaxValue)]
        public decimal HourlyRate { get; set; }

        [Range(0, double.MaxValue)]
        public decimal? MaximumMonthlyHours { get; set; } = 160;

        [StringLength(20)]
        public string Status { get; set; } = "Active";

        [StringLength(1000)]
        public string? Terms { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public DateTime? ModifiedDate { get; set; }

        // Navigation properties
        public virtual ApplicationUser? User { get; set; }
    }
}