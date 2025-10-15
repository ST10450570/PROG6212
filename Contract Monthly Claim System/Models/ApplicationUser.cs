using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace Contract_Monthly_Claim_System.Models
{
    public class ApplicationUser : IdentityUser
    {
        [Required]
        [StringLength(100)]
        [Display(Name = "First Name")]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        [Display(Name = "Last Name")]
        public string LastName { get; set; } = string.Empty;

        [StringLength(20)]
        [Display(Name = "Employee Number")]
        public string? EmployeeNumber { get; set; }

        public int? DepartmentId { get; set; }

        [Display(Name = "Profile Picture")]
        public string? ProfilePicture { get; set; }

        [Display(Name = "Date of Birth")]
        [DataType(DataType.Date)]
        public DateTime? DateOfBirth { get; set; }

        [StringLength(500)]
        public string? Bio { get; set; }

        public bool IsActive { get; set; } = true;
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public DateTime? LastLoginDate { get; set; }
        public DateTime? LastPasswordChangeDate { get; set; }

        // Navigation properties
        public virtual Department? Department { get; set; }
        public virtual ICollection<Claim> Claims { get; set; } = new List<Claim>();
        public virtual ICollection<ApprovalLog> ApprovalActions { get; set; } = new List<ApprovalLog>();
        public virtual ICollection<Notification> Notifications { get; set; } = new List<Notification>();

        // Computed properties
        [Display(Name = "Full Name")]
        public string FullName => $"{FirstName} {LastName}";

        public string Initials => $"{FirstName[0]}{LastName[0]}".ToUpper();
    }
}