using Microsoft.AspNetCore.Identity;

namespace Contract_Monthly_Claim_System.Models
{
    public class ApplicationRole : IdentityRole
    {
        public string Description { get; set; } = string.Empty;
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public bool IsSystemRole { get; set; } = false;
    }
}