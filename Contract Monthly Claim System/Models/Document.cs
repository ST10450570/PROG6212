using System.ComponentModel.DataAnnotations;

namespace Contract_Monthly_Claim_System.Models
{
    public class Document
    {
        public int DocumentId { get; set; }

        [Required]
        [StringLength(255)]
        public string FileName { get; set; } = string.Empty;

        [StringLength(500)]
        public string? FilePath { get; set; }

        [StringLength(50)]
        public string? FileType { get; set; }

        public long? FileSize { get; set; }

        public DateTime UploadDate { get; set; } = DateTime.Now;

        [StringLength(200)]
        public string? Description { get; set; }

        // Foreign key
        public int ClaimId { get; set; }

        // Navigation property
        public virtual Claim? Claim { get; set; }

        // Methods
        public void UploadFile()
        {
            UploadDate = DateTime.Now;
        }

        public string GetFileSizeFormatted()
        {
            if (!FileSize.HasValue) return "0 KB";

            string[] sizes = { "B", "KB", "MB", "GB" };
            int order = 0;
            long size = FileSize.Value;
            while (size >= 1024 && order < sizes.Length - 1)
            {
                order++;
                size /= 1024;
            }
            return $"{size:0.##} {sizes[order]}";
        }
    }
}