using Contract_Monthly_Claim_System.Models;
using Contract_Monthly_Claim_System.Data;
using Microsoft.EntityFrameworkCore;

namespace Contract_Monthly_Claim_System.Services
{
    public interface IAuditService
    {
        Task LogActionAsync(string userId, string tableName, string action, string recordId, string oldValues = null, string newValues = null);
        Task<IEnumerable<AuditLog>> GetAuditLogsAsync(DateTime? fromDate = null, DateTime? toDate = null, string userId = null);
    }

    public class AuditService : IAuditService
    {
        private readonly ApplicationDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AuditService(ApplicationDbContext context, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task LogActionAsync(string userId, string tableName, string action, string recordId, string oldValues = null, string newValues = null)
        {
            var auditLog = new AuditLog
            {
                UserId = userId,
                TableName = tableName,
                Action = action,
                RecordId = recordId,
                OldValues = oldValues,
                NewValues = newValues,
                Timestamp = DateTime.Now,
                IpAddress = _httpContextAccessor.HttpContext?.Connection?.RemoteIpAddress?.ToString(),
                UserAgent = _httpContextAccessor.HttpContext?.Request?.Headers["User-Agent"].ToString()
            };

            _context.AuditLogs.Add(auditLog);
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<AuditLog>> GetAuditLogsAsync(DateTime? fromDate = null, DateTime? toDate = null, string userId = null)
        {
            var query = _context.AuditLogs
                .Include(a => a.User)
                .AsQueryable();

            if (fromDate.HasValue)
                query = query.Where(a => a.Timestamp >= fromDate.Value);

            if (toDate.HasValue)
                query = query.Where(a => a.Timestamp <= toDate.Value);

            if (!string.IsNullOrEmpty(userId))
                query = query.Where(a => a.UserId == userId);

            return await query
                .OrderByDescending(a => a.Timestamp)
                .ToListAsync();
        }
    }
}