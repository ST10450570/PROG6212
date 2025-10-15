using Contract_Monthly_Claim_System.Data;
using Contract_Monthly_Claim_System.Models;
using Microsoft.EntityFrameworkCore;

namespace Contract_Monthly_Claim_System.Services
{
    public interface IDashboardService
    {
        Task<DashboardStats> GetAdminStatsAsync();
        Task<DashboardStats> GetCoordinatorStatsAsync(string departmentId);
        Task<DashboardStats> GetUserStatsAsync(string userId);
    }

    public class DashboardService : IDashboardService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<DashboardService> _logger;

        public DashboardService(ApplicationDbContext context, ILogger<DashboardService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<DashboardStats> GetAdminStatsAsync()
        {
            try
            {
                var claims = await _context.Claims.ToListAsync();

                var totalClaims = claims.Count;
                var approvedClaims = claims.Count(c => c.Status == "Approved");
                var pendingClaims = claims.Count(c => c.Status == "Submitted" || c.Status == "UnderReview");
                var rejectedClaims = claims.Count(c => c.Status == "Rejected");
                var totalAmount = claims.Where(c => c.Status == "Approved").Sum(c => c.TotalAmount);
                var pendingAmount = claims.Where(c => c.Status == "Submitted" || c.Status == "UnderReview").Sum(c => c.TotalAmount);
                var recentClaims = claims.Count(c => c.ClaimDate >= DateTime.UtcNow.AddDays(-30));

                return new DashboardStats
                {
                    TotalClaims = totalClaims,
                    ApprovedClaims = approvedClaims,
                    PendingClaims = pendingClaims,
                    RejectedClaims = rejectedClaims,
                    TotalAmount = totalAmount,
                    PendingAmount = pendingAmount,
                    RecentClaimsCount = recentClaims
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting admin dashboard stats");
                throw;
            }
        }

        public async Task<DashboardStats> GetCoordinatorStatsAsync(string departmentId)
        {
            try
            {
                if (!int.TryParse(departmentId, out int deptId))
                {
                    return new DashboardStats();
                }

                var claims = await _context.Claims
                    .Where(c => c.DepartmentId == deptId)
                    .ToListAsync();

                var totalClaims = claims.Count;
                var approvedClaims = claims.Count(c => c.Status == "Approved");
                var pendingClaims = claims.Count(c => c.Status == "Submitted" || c.Status == "UnderReview");
                var rejectedClaims = claims.Count(c => c.Status == "Rejected");
                var totalAmount = claims.Where(c => c.Status == "Approved").Sum(c => c.TotalAmount);
                var pendingAmount = claims.Where(c => c.Status == "Submitted" || c.Status == "UnderReview").Sum(c => c.TotalAmount);
                var recentClaims = claims.Count(c => c.ClaimDate >= DateTime.UtcNow.AddDays(-30));

                return new DashboardStats
                {
                    TotalClaims = totalClaims,
                    ApprovedClaims = approvedClaims,
                    PendingClaims = pendingClaims,
                    RejectedClaims = rejectedClaims,
                    TotalAmount = totalAmount,
                    PendingAmount = pendingAmount,
                    RecentClaimsCount = recentClaims
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting coordinator dashboard stats for department {DepartmentId}", departmentId);
                throw;
            }
        }

        public async Task<DashboardStats> GetUserStatsAsync(string userId)
        {
            try
            {
                var claims = await _context.Claims
                    .Where(c => c.UserId == userId)
                    .ToListAsync();

                var totalClaims = claims.Count;
                var approvedClaims = claims.Count(c => c.Status == "Approved");
                var pendingClaims = claims.Count(c => c.Status == "Submitted" || c.Status == "UnderReview");
                var rejectedClaims = claims.Count(c => c.Status == "Rejected");
                var totalAmount = claims.Where(c => c.Status == "Approved").Sum(c => c.TotalAmount);
                var pendingAmount = claims.Where(c => c.Status == "Submitted" || c.Status == "UnderReview").Sum(c => c.TotalAmount);
                var recentClaims = claims.Count(c => c.ClaimDate >= DateTime.UtcNow.AddDays(-30));

                return new DashboardStats
                {
                    TotalClaims = totalClaims,
                    ApprovedClaims = approvedClaims,
                    PendingClaims = pendingClaims,
                    RejectedClaims = rejectedClaims,
                    TotalAmount = totalAmount,
                    PendingAmount = pendingAmount,
                    RecentClaimsCount = recentClaims
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user dashboard stats for user {UserId}", userId);
                throw;
            }
        }
    }
}