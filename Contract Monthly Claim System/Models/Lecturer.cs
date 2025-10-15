using System.ComponentModel.DataAnnotations;

namespace Contract_Monthly_Claim_System.Models
{
    public class Lecturer
    {
        public int LecturerId { get; set; }

        [Required]
        [StringLength(100)]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string LastName { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [StringLength(20)]
        public string? EmployeeNumber { get; set; }

        [StringLength(20)]
        public string Role { get; set; } = "Lecturer"; // Lecturer, Coordinator, Manager

        public int? DepartmentId { get; set; }

        public bool IsActive { get; set; } = true;
        public DateTime CreatedDate { get; set; } = DateTime.Now;

        // Navigation properties
        public virtual Department? Department { get; set; }

        // Computed properties
        public string FullName => $"{FirstName} {LastName}";
    }
}