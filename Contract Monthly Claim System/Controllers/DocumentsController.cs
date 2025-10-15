using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Contract_Monthly_Claim_System.Data;
using Contract_Monthly_Claim_System.Models;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace Contract_Monthly_Claim_System.Controllers
{
    [Authorize]
    public class DocumentsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _environment;

        public DocumentsController(ApplicationDbContext context, IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }

        // GET: Documents/Upload/5
        public async Task<IActionResult> Upload(int claimId)
        {
            var claim = await _context.Claims
                .Include(c => c.User)
                .FirstOrDefaultAsync(c => c.ClaimId == claimId);

            if (claim == null)
            {
                return NotFound();
            }

            // Check if user owns the claim or has appropriate permissions
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (claim.UserId != userId && !User.IsInRole("Coordinator") && !User.IsInRole("Manager"))
            {
                return Forbid();
            }

            ViewBag.ClaimId = claimId;
            ViewBag.ClaimReference = $"Claim #{claim.ClaimNumber}";
            return View();
        }

        // POST: Documents/Upload
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Upload(int claimId, IFormFile file, string description)
        {
            if (file == null || file.Length == 0)
            {
                ModelState.AddModelError("", "Please select a file to upload.");
                ViewBag.ClaimId = claimId;
                return View();
            }

            // Validate file size (10MB limit)
            if (file.Length > 10 * 1024 * 1024)
            {
                ModelState.AddModelError("", "File size cannot exceed 10MB.");
                ViewBag.ClaimId = claimId;
                return View();
            }

            // Validate file type
            var allowedExtensions = new[] { ".pdf", ".docx", ".xlsx", ".jpg", ".jpeg", ".png" };
            var fileExtension = Path.GetExtension(file.FileName).ToLower();
            if (!allowedExtensions.Contains(fileExtension))
            {
                ModelState.AddModelError("", "Only PDF, DOCX, XLSX, JPG, and PNG files are allowed.");
                ViewBag.ClaimId = claimId;
                return View();
            }

            try
            {
                // Verify claim exists and user has access
                var claim = await _context.Claims
                    .Include(c => c.User)
                    .FirstOrDefaultAsync(c => c.ClaimId == claimId);

                if (claim == null)
                {
                    return NotFound();
                }

                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (claim.UserId != userId && !User.IsInRole("Coordinator") && !User.IsInRole("Manager"))
                {
                    return Forbid();
                }

                // Create uploads directory if it doesn't exist
                var uploadsPath = Path.Combine(_environment.WebRootPath, "uploads", "documents");
                if (!Directory.Exists(uploadsPath))
                {
                    Directory.CreateDirectory(uploadsPath);
                }

                // Generate unique filename
                var fileName = $"{Guid.NewGuid()}{fileExtension}";
                var filePath = Path.Combine(uploadsPath, fileName);

                // Save file
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                // Create document record
                var document = new Document
                {
                    FileName = file.FileName,
                    FilePath = $"/uploads/documents/{fileName}",
                    FileType = fileExtension.TrimStart('.'),
                    FileSize = file.Length,
                    Description = description,
                    ClaimId = claimId,
                    UploadDate = DateTime.Now
                };

                _context.Documents.Add(document);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Document uploaded successfully!";
                return RedirectToAction("Details", "Claims", new { id = claimId });
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Error uploading file: {ex.Message}");
                ViewBag.ClaimId = claimId;
                return View();
            }
        }

        // GET: Documents/Download/5
        public async Task<IActionResult> Download(int id)
        {
            var document = await _context.Documents
                .Include(d => d.Claim)
                .FirstOrDefaultAsync(d => d.DocumentId == id);

            if (document == null)
            {
                return NotFound();
            }

            // Check permissions
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (document.Claim.UserId != userId && !User.IsInRole("Coordinator") && !User.IsInRole("Manager"))
            {
                return Forbid();
            }

            var filePath = Path.Combine(_environment.WebRootPath, document.FilePath.TrimStart('/'));
            if (!System.IO.File.Exists(filePath))
            {
                return NotFound();
            }

            var memory = new MemoryStream();
            using (var stream = new FileStream(filePath, FileMode.Open))
            {
                await stream.CopyToAsync(memory);
            }
            memory.Position = 0;

            return File(memory, GetContentType(document.FilePath), document.FileName);
        }

        // POST: Documents/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var document = await _context.Documents
                .Include(d => d.Claim)
                .FirstOrDefaultAsync(d => d.DocumentId == id);

            if (document == null)
            {
                return NotFound();
            }

            var claimId = document.ClaimId;

            // Check permissions
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (document.Claim.UserId != userId && !User.IsInRole("Coordinator") && !User.IsInRole("Manager"))
            {
                return Forbid();
            }

            try
            {
                // Delete physical file
                var filePath = Path.Combine(_environment.WebRootPath, document.FilePath.TrimStart('/'));
                if (System.IO.File.Exists(filePath))
                {
                    System.IO.File.Delete(filePath);
                }

                _context.Documents.Remove(document);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Document deleted successfully!";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error deleting document: {ex.Message}";
            }

            return RedirectToAction("Details", "Claims", new { id = claimId });
        }

        private static string GetContentType(string path)
        {
            var types = new Dictionary<string, string>
            {
                { ".pdf", "application/pdf" },
                { ".docx", "application/vnd.openxmlformats-officedocument.wordprocessingml.document" },
                { ".xlsx", "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet" },
                { ".jpg", "image/jpeg" },
                { ".jpeg", "image/jpeg" },
                { ".png", "image/png" }
            };

            var ext = Path.GetExtension(path).ToLowerInvariant();
            return types.TryGetValue(ext, out var contentType) ? contentType : "application/octet-stream";
        }
    }
}