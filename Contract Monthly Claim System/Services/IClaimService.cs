using Contract_Monthly_Claim_System.Models;

namespace Contract_Monthly_Claim_System.Services
{
    public interface IClaimService
    {
        Task<Claim> CreateClaimAsync(Claim claim, string userId);
        Task<Claim> SubmitClaimAsync(int claimId, string userId);
        Task<Claim> ApproveClaimAsync(int claimId, string approverId, string comments);
        Task<Claim> RejectClaimAsync(int claimId, string approverId, string reason, string comments);
        Task<Claim> ReturnClaimAsync(int claimId, string approverId, string comments);
        Task<Claim> GetClaimByIdAsync(int claimId, string userId = null);
        Task<IEnumerable<Claim>> GetUserClaimsAsync(string userId);
        Task<IEnumerable<Claim>> GetPendingClaimsAsync(string departmentId = null);
        Task<IEnumerable<Claim>> GetClaimsByStatusAsync(string status);
        Task<DashboardStats> GetDashboardStatsAsync(string userId);
        Task<bool> ValidateClaimAsync(Claim claim);
    }

    public class DashboardStats
    {
        public int TotalClaims { get; set; }
        public int ApprovedClaims { get; set; }
        public int PendingClaims { get; set; }
        public int RejectedClaims { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal PendingAmount { get; set; }
        public int RecentClaimsCount { get; set; }
    }
}