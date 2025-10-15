using Contract_Monthly_Claim_System.Data;
using Contract_Monthly_Claim_System.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace Contract_Monthly_Claim_System.Services
{
    public class ClaimService : IClaimService
    {
        private readonly ApplicationDbContext _context;
        private readonly IEmailService _emailService;
        private readonly IAuditService _auditService;
        private readonly ILogger<ClaimService> _logger;

        public ClaimService(
            ApplicationDbContext context,
            IEmailService emailService,
            IAuditService auditService,
            ILogger<ClaimService> logger)
        {
            _context = context;
            _emailService = emailService;
            _auditService = auditService;
            _logger = logger;
        }

        public async Task<Claim> CreateClaimAsync(Claim claim, string userId)
        {
            try
            {
                claim.UserId = userId;
                claim.Status = "Draft";
                claim.CalculateTotal();

                if (!await ValidateClaimAsync(claim))
                    throw new InvalidOperationException("Claim validation failed");

                _context.Claims.Add(claim);
                await _context.SaveChangesAsync();

                await _auditService.LogActionAsync(userId, "Claims", "CREATE", claim.ClaimId.ToString());

                _logger.LogInformation("Claim {ClaimNumber} created by user {UserId}", claim.ClaimNumber, userId);
                return claim;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating claim for user {UserId}", userId);
                throw;
            }
        }

        public async Task<Claim> SubmitClaimAsync(int claimId, string userId)
        {
            var claim = await _context.Claims
                .Include(c => c.User)
                .FirstOrDefaultAsync(c => c.ClaimId == claimId && c.UserId == userId);

            if (claim == null)
                throw new ArgumentException("Claim not found or access denied");

            if (!claim.CanSubmit)
                throw new InvalidOperationException("Claim cannot be submitted in its current state");

            claim.Submit();
            await _context.SaveChangesAsync();

            // Create approval log
            var approvalLog = new ApprovalLog
            {
                ClaimId = claimId,
                ApproverId = userId,
                Action = "Submitted",
                Comments = "Claim submitted for review",
                ActionDate = DateTime.UtcNow
            };
            _context.ApprovalLogs.Add(approvalLog);

            await _context.SaveChangesAsync();

            // Send notifications
            if (!string.IsNullOrEmpty(claim.User?.Email))
            {
                await _emailService.SendClaimSubmittedEmailAsync(claim, claim.User.Email);
            }

            await _auditService.LogActionAsync(userId, "Claims", "SUBMIT", claim.ClaimId.ToString());

            _logger.LogInformation("Claim {ClaimNumber} submitted by user {UserId}", claim.ClaimNumber, userId);
            return claim;
        }

        public async Task<Claim> ApproveClaimAsync(int claimId, string approverId, string comments)
        {
            var claim = await _context.Claims
                .Include(c => c.User)
                .FirstOrDefaultAsync(c => c.ClaimId == claimId);

            if (claim == null)
                throw new ArgumentException("Claim not found");

            if (!claim.CanApprove)
                throw new InvalidOperationException("Claim cannot be approved in its current state");

            claim.Approve(approverId);

            // Log approval action
            var approvalLog = new ApprovalLog
            {
                ClaimId = claimId,
                ApproverId = approverId,
                Action = "Approved",
                Comments = comments,
                ActionDate = DateTime.UtcNow
            };
            _context.ApprovalLogs.Add(approvalLog);

            await _context.SaveChangesAsync();

            // Send notification
            if (!string.IsNullOrEmpty(claim.User?.Email))
            {
                await _emailService.SendClaimApprovedEmailAsync(claim, claim.User.Email);
            }

            await _auditService.LogActionAsync(approverId, "Claims", "APPROVE", claim.ClaimId.ToString());

            _logger.LogInformation("Claim {ClaimNumber} approved by {ApproverId}", claim.ClaimNumber, approverId);
            return claim;
        }

        public async Task<Claim> RejectClaimAsync(int claimId, string approverId, string reason, string comments)
        {
            var claim = await _context.Claims
                .Include(c => c.User)
                .FirstOrDefaultAsync(c => c.ClaimId == claimId);

            if (claim == null)
                throw new ArgumentException("Claim not found");

            if (!claim.CanReject)
                throw new InvalidOperationException("Claim cannot be rejected in its current state");

            claim.Reject(approverId, reason);

            // Log rejection action
            var approvalLog = new ApprovalLog
            {
                ClaimId = claimId,
                ApproverId = approverId,
                Action = "Rejected",
                Comments = $"{reason}. {comments}",
                ActionDate = DateTime.UtcNow
            };
            _context.ApprovalLogs.Add(approvalLog);

            await _context.SaveChangesAsync();

            // Send notification
            if (!string.IsNullOrEmpty(claim.User?.Email))
            {
                await _emailService.SendClaimRejectedEmailAsync(claim, claim.User.Email, reason);
            }

            await _auditService.LogActionAsync(approverId, "Claims", "REJECT", claim.ClaimId.ToString());

            _logger.LogInformation("Claim {ClaimNumber} rejected by {ApproverId}", claim.ClaimNumber, approverId);
            return claim;
        }

        public async Task<Claim> ReturnClaimAsync(int claimId, string approverId, string comments)
        {
            var claim = await _context.Claims
                .Include(c => c.User)
                .FirstOrDefaultAsync(c => c.ClaimId == claimId);

            if (claim == null)
                throw new ArgumentException("Claim not found");

            claim.ReturnForCorrection();

            // Log return action
            var approvalLog = new ApprovalLog
            {
                ClaimId = claimId,
                ApproverId = approverId,
                Action = "Returned",
                Comments = comments,
                ActionDate = DateTime.UtcNow
            };
            _context.ApprovalLogs.Add(approvalLog);

            await _context.SaveChangesAsync();

            // Send notification
            if (!string.IsNullOrEmpty(claim.User?.Email))
            {
                await _emailService.SendClaimReturnedEmailAsync(claim, claim.User.Email, comments);
            }

            await _auditService.LogActionAsync(approverId, "Claims", "RETURN", claim.ClaimId.ToString());

            _logger.LogInformation("Claim {ClaimNumber} returned for correction by {ApproverId}", claim.ClaimNumber, approverId);
            return claim;
        }

        public async Task<Claim> GetClaimByIdAsync(int claimId, string userId = null)
        {
            var query = _context.Claims
                .Include(c => c.User)
                .Include(c => c.Department)
                .Include(c => c.Documents)
                .Include(c => c.ApprovalLogs)
                    .ThenInclude(a => a.Approver)
                .AsQueryable();

            if (!string.IsNullOrEmpty(userId))
            {
                // Users can only see their own claims unless they are coordinators/managers
                var user = await _context.Users.FindAsync(userId);
                if (user != null && !await IsUserCoordinatorOrManager(userId))
                {
                    query = query.Where(c => c.UserId == userId);
                }
            }

            return await query.FirstOrDefaultAsync(c => c.ClaimId == claimId);
        }

        public async Task<IEnumerable<Claim>> GetUserClaimsAsync(string userId)
        {
            return await _context.Claims
                .Where(c => c.UserId == userId)
                .Include(c => c.Department)
                .OrderByDescending(c => c.ClaimDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<Claim>> GetPendingClaimsAsync(string departmentId = null)
        {
            var query = _context.Claims
                .Include(c => c.User)
                .Include(c => c.Department)
                .Where(c => c.Status == "Submitted" || c.Status == "UnderReview");

            if (!string.IsNullOrEmpty(departmentId) && int.TryParse(departmentId, out int deptId))
            {
                query = query.Where(c => c.DepartmentId == deptId);
            }

            return await query
                .OrderBy(c => c.SubmittedDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<Claim>> GetClaimsByStatusAsync(string status)
        {
            return await _context.Claims
                .Include(c => c.User)
                .Include(c => c.Department)
                .Where(c => c.Status == status)
                .OrderByDescending(c => c.ClaimDate)
                .ToListAsync();
        }

        public async Task<DashboardStats> GetDashboardStatsAsync(string userId)
        {
            var userClaims = _context.Claims.Where(c => c.UserId == userId);
            var claimsList = await userClaims.ToListAsync();

            return new DashboardStats
            {
                TotalClaims = claimsList.Count,
                ApprovedClaims = claimsList.Count(c => c.Status == "Approved"),
                PendingClaims = claimsList.Count(c => c.Status == "Submitted" || c.Status == "UnderReview"),
                RejectedClaims = claimsList.Count(c => c.Status == "Rejected"),
                TotalAmount = claimsList.Where(c => c.Status == "Approved").Sum(c => c.TotalAmount),
                PendingAmount = claimsList.Where(c => c.Status == "Submitted" || c.Status == "UnderReview").Sum(c => c.TotalAmount),
                RecentClaimsCount = claimsList.Count(c => c.ClaimDate >= DateTime.UtcNow.AddDays(-30))
            };
        }

        public async Task<bool> ValidateClaimAsync(Claim claim)
        {
            // Check for overlapping claims in the same period
            var overlappingClaims = await _context.Claims
                .Where(c => c.UserId == claim.UserId &&
                           c.ClaimId != claim.ClaimId &&
                           c.PeriodStart <= claim.PeriodEnd &&
                           c.PeriodEnd >= claim.PeriodStart)
                .AnyAsync();

            if (overlappingClaims)
                throw new InvalidOperationException("A claim already exists for this period");

            // Check maximum monthly hours
            var monthlyClaims = await _context.Claims
                .Where(c => c.UserId == claim.UserId &&
                           c.PeriodStart.Month == claim.PeriodStart.Month &&
                           c.PeriodStart.Year == claim.PeriodStart.Year)
                .ToListAsync();

            var monthlyHours = monthlyClaims.Sum(c => c.HoursWorked);

            if (monthlyHours + claim.HoursWorked > 200) // Max 200 hours per month
                throw new InvalidOperationException("Monthly hours limit exceeded");

            return true;
        }

        private async Task<bool> IsUserCoordinatorOrManager(string userId)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null) return false;

            // Check if user has Coordinator or Manager role
            var userRoles = await _context.UserRoles
                .Where(ur => ur.UserId == userId)
                .Join(_context.Roles,
                      ur => ur.RoleId,
                      r => r.Id,
                      (ur, r) => r.Name)
                .ToListAsync();

            return userRoles.Any(role => role == "Coordinator" || role == "Manager");
        }
    }
}