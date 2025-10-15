using Contract_Monthly_Claim_System.Data;
using Contract_Monthly_Claim_System.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Contract_Monthly_Claim_System.Services
{
    public interface IFileService
    {
        Task<Document> SaveDocumentAsync(IFormFile file, int claimId, string description);
        Task<FileResult> DownloadDocumentAsync(int documentId);
        Task<bool> DeleteDocumentAsync(int documentId);
        Task<IEnumerable<Document>> GetClaimDocumentsAsync(int claimId);
    }

    public class FileService : IFileService
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _environment;
        private readonly ILogger<FileService> _logger;

        public FileService(ApplicationDbContext context, IWebHostEnvironment environment, ILogger<FileService> logger)
        {
            _context = context;
            _environment = environment;
            _logger = logger;
        }

        public async Task<Document> SaveDocumentAsync(IFormFile file, int claimId, string description)
        {
            try
            {
                // Validate file
                if (file == null || file.Length == 0)
                    throw new ArgumentException("No file provided");

                // Validate file size (10MB max)
                if (file.Length > 10 * 1024 * 1024)
                    throw new ArgumentException("File size exceeds 10MB limit");

                // Validate file type
                var allowedExtensions = new[] { ".pdf", ".doc", ".docx", ".xls", ".xlsx", ".jpg", ".jpeg", ".png" };
                var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();
                if (!allowedExtensions.Contains(fileExtension))
                    throw new ArgumentException("File type not allowed");

                // Create uploads directory if it doesn't exist
                var uploadsPath = Path.Combine(_environment.WebRootPath, "uploads", "documents");
                if (!Directory.Exists(uploadsPath))
                    Directory.CreateDirectory(uploadsPath);

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

                _logger.LogInformation("Document {FileName} saved successfully for claim {ClaimId}", document.FileName, claimId);
                return document;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving document for claim {ClaimId}", claimId);
                throw;
            }
        }

        public async Task<FileResult> DownloadDocumentAsync(int documentId)
        {
            var document = await _context.Documents.FindAsync(documentId);
            if (document == null || !System.IO.File.Exists(document.FilePath))
                throw new FileNotFoundException("Document not found");

            var fileBytes = await System.IO.File.ReadAllBytesAsync(document.FilePath);
            return new FileContentResult(fileBytes, "application/octet-stream")
            {
                FileDownloadName = document.FileName
            };
        }

        public async Task<bool> DeleteDocumentAsync(int documentId)
        {
            var document = await _context.Documents.FindAsync(documentId);
            if (document == null) return false;

            try
            {
                // Delete physical file
                if (System.IO.File.Exists(document.FilePath))
                    System.IO.File.Delete(document.FilePath);

                // Delete database record
                _context.Documents.Remove(document);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Document {DocumentId} deleted successfully", documentId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting document {DocumentId}", documentId);
                return false;
            }
        }

        public async Task<IEnumerable<Document>> GetClaimDocumentsAsync(int claimId)
        {
            return await _context.Documents
                .Where(d => d.ClaimId == claimId)
                .OrderByDescending(d => d.UploadDate)
                .ToListAsync();
        }
    }
}