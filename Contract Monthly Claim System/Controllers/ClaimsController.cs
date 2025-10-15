using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Contract_Monthly_Claim_System.Models;
using Contract_Monthly_Claim_System.Services;
using Contract_Monthly_Claim_System.Data;
using System.Security.Claims;

namespace Contract_Monthly_Claim_System.Controllers
{
    [Authorize]
    public class ClaimsController : Controller
    {
        private readonly IClaimService _claimService;
        private readonly IFileService _fileService;
        private readonly ApplicationDbContext _context;
        private readonly ILogger<ClaimsController> _logger;

        public ClaimsController(
            IClaimService claimService,
            IFileService fileService,
            ApplicationDbContext context,
            ILogger<ClaimsController> logger)
        {
            _claimService = claimService;
            _fileService = fileService;
            _context = context;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> Dashboard()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await _context.Users
                .Include(u => u.Department)
                .FirstOrDefaultAsync(u => u.Id == userId);

            ViewBag.UserName = user?.FullName ?? "User";
            ViewBag.Department = user?.Department?.Name ?? "N/A";
            ViewBag.UserRole = User.FindFirstValue(ClaimTypes.Role) ?? "User";
            ViewBag.UserInitials = user?.Initials ?? "U";

            var stats = await _claimService.GetDashboardStatsAsync(userId);
            return View(stats);
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var claims = await _claimService.GetUserClaimsAsync(userId);
            return View(claims);
        }

        [HttpGet]
        public IActionResult Create()
        {
            var claim = new Models.Claim
            {
                PeriodStart = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1),
                PeriodEnd = new DateTime(DateTime.Now.Year, DateTime.Now.Month,
                    DateTime.DaysInMonth(DateTime.Now.Year, DateTime.Now.Month)),
                HourlyRate = 350 // Default rate
            };
            return View(claim);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Models.Claim claim)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                    var user = await _context.Users
                        .Include(u => u.Department)
                        .FirstOrDefaultAsync(u => u.Id == userId);

                    claim.DepartmentId = user?.DepartmentId;
                    var createdClaim = await _claimService.CreateClaimAsync(claim, userId);

                    TempData["Success"] = $"Claim {createdClaim.ClaimNumber} created successfully!";
                    return RedirectToAction(nameof(Details), new { id = createdClaim.ClaimId });
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error creating claim");
                    ModelState.AddModelError("", $"An error occurred while creating the claim: {ex.Message}");
                }
            }

            return View(claim);
        }

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var claim = await _claimService.GetClaimByIdAsync(id, userId);

            if (claim == null)
            {
                return NotFound();
            }

            // Check authorization
            if (claim.UserId != userId && !User.IsInRole("Coordinator") && !User.IsInRole("Manager"))
            {
                return Forbid();
            }

            return View(claim);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Submit(int id)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var claim = await _claimService.SubmitClaimAsync(id, userId);

                TempData["Success"] = $"Claim {claim.ClaimNumber} submitted successfully!";
                return RedirectToAction(nameof(Details), new { id = claim.ClaimId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error submitting claim {ClaimId}", id);
                TempData["Error"] = $"An error occurred while submitting the claim: {ex.Message}";
                return RedirectToAction(nameof(Details), new { id });
            }
        }

        [Authorize(Roles = "Coordinator,Manager")]
        [HttpGet]
        public async Task<IActionResult> Pending()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await _context.Users
                .Include(u => u.Department)
                .FirstOrDefaultAsync(u => u.Id == userId);

            string departmentId = user?.DepartmentId?.ToString();
            var claims = await _claimService.GetPendingClaimsAsync(departmentId);
            ViewBag.UserRole = User.IsInRole("Manager") ? "Manager" : "Coordinator";

            return View(claims);
        }

        [Authorize(Roles = "Coordinator,Manager")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Approve(int id, string comments)
        {
            try
            {
                var approverId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var claim = await _claimService.ApproveClaimAsync(id, approverId, comments);

                TempData["Success"] = $"Claim {claim.ClaimNumber} approved successfully!";
                return RedirectToAction(nameof(Pending));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error approving claim {ClaimId}", id);
                TempData["Error"] = $"An error occurred while approving the claim: {ex.Message}";
                return RedirectToAction(nameof(Pending));
            }
        }

        [Authorize(Roles = "Coordinator,Manager")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reject(int id, string reason, string comments)
        {
            try
            {
                var approverId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var claim = await _claimService.RejectClaimAsync(id, approverId, reason, comments);

                TempData["Success"] = $"Claim {claim.ClaimNumber} has been rejected.";
                return RedirectToAction(nameof(Pending));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error rejecting claim {ClaimId}", id);
                TempData["Error"] = $"An error occurred while rejecting the claim: {ex.Message}";
                return RedirectToAction(nameof(Pending));
            }
        }

        [Authorize(Roles = "Coordinator,Manager")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Return(int id, string comments)
        {
            try
            {
                var approverId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var claim = await _claimService.ReturnClaimAsync(id, approverId, comments);

                TempData["Success"] = $"Claim {claim.ClaimNumber} has been returned for correction.";
                return RedirectToAction(nameof(Pending));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error returning claim {ClaimId}", id);
                TempData["Error"] = $"An error occurred while returning the claim: {ex.Message}";
                return RedirectToAction(nameof(Pending));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UploadDocument(int claimId, IFormFile file, string description)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var claim = await _claimService.GetClaimByIdAsync(claimId, userId);

                if (claim == null || claim.UserId != userId)
                {
                    return Forbid();
                }

                if (file != null && file.Length > 0)
                {
                    var document = await _fileService.SaveDocumentAsync(file, claimId, description);
                    TempData["Success"] = "Document uploaded successfully!";
                }
                else
                {
                    TempData["Error"] = "Please select a file to upload.";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading document for claim {ClaimId}", claimId);
                TempData["Error"] = $"An error occurred while uploading the document: {ex.Message}";
            }

            return RedirectToAction(nameof(Details), new { id = claimId });
        }

        // API endpoints for dashboard
        [HttpGet]
        public async Task<JsonResult> GetDashboardStats()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var stats = await _claimService.GetDashboardStatsAsync(userId);
            return Json(stats);
        }

        [HttpGet]
        public async Task<IActionResult> GetRecentClaims()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var claims = await _claimService.GetUserClaimsAsync(userId);
            var recentClaims = claims.Take(5).Select(c => new
            {
                c.ClaimNumber,
                c.ClaimDate,
                c.TotalAmount,
                c.Status,
                Period = c.ClaimPeriod
            });

            return Json(recentClaims);
        }
    }
}