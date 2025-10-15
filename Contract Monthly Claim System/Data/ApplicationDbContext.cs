using Contract_Monthly_Claim_System.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Contract_Monthly_Claim_System.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, string>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<Department> Departments { get; set; }
        public DbSet<Claim> Claims { get; set; }
        public DbSet<Document> Documents { get; set; }
        public DbSet<ApprovalLog> ApprovalLogs { get; set; }
        public DbSet<AuditLog> AuditLogs { get; set; }
        public DbSet<SystemSetting> SystemSettings { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<Models.Contract> Contracts { get; set; }
        public DbSet<Payment> Payments { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Configure relationships with cascade behaviors
            builder.Entity<ApplicationUser>(b =>
            {
                b.HasOne(u => u.Department)
                 .WithMany(d => d.Users)
                 .HasForeignKey(u => u.DepartmentId)
                 .OnDelete(DeleteBehavior.Restrict);

                b.HasMany(u => u.Claims)
                 .WithOne(c => c.User)
                 .HasForeignKey(c => c.UserId)
                 .OnDelete(DeleteBehavior.Cascade);
            });

            builder.Entity<Claim>(b =>
            {
                b.HasOne(c => c.User)
                 .WithMany(u => u.Claims)
                 .HasForeignKey(c => c.UserId)
                 .OnDelete(DeleteBehavior.Cascade);

                b.HasOne(c => c.Department)
                 .WithMany()
                 .HasForeignKey(c => c.DepartmentId)
                 .OnDelete(DeleteBehavior.Restrict);

                b.HasIndex(c => c.ClaimNumber).IsUnique();

                b.Property(c => c.TotalAmount)
                 .HasPrecision(18, 2);

                b.Property(c => c.HourlyRate)
                 .HasPrecision(18, 2);

                b.Property(c => c.HoursWorked)
                 .HasPrecision(8, 2);
            });

            builder.Entity<Document>(b =>
            {
                b.HasOne(d => d.Claim)
                 .WithMany(c => c.Documents)
                 .HasForeignKey(d => d.ClaimId)
                 .OnDelete(DeleteBehavior.Cascade);
            });

            builder.Entity<ApprovalLog>(b =>
            {
                b.HasOne(a => a.Claim)
                 .WithMany(c => c.ApprovalLogs)
                 .HasForeignKey(a => a.ClaimId)
                 .OnDelete(DeleteBehavior.Cascade);

                b.HasOne(a => a.Approver)
                 .WithMany(u => u.ApprovalActions)
                 .HasForeignKey(a => a.ApproverId)
                 .OnDelete(DeleteBehavior.Restrict);
            });

            builder.Entity<AuditLog>(b =>
            {
                b.HasOne(a => a.User)
                 .WithMany()
                 .HasForeignKey(a => a.UserId)
                 .OnDelete(DeleteBehavior.Restrict);
            });

            builder.Entity<Notification>(b =>
            {
                b.HasOne(n => n.User)
                 .WithMany(u => u.Notifications)
                 .HasForeignKey(n => n.UserId)
                 .OnDelete(DeleteBehavior.Cascade);
            });

            builder.Entity<Payment>(b =>
            {
                b.HasOne(p => p.Claim)
                 .WithMany(c => c.Payments)
                 .HasForeignKey(p => p.ClaimId)
                 .OnDelete(DeleteBehavior.Restrict);

                b.Property(p => p.Amount)
                 .HasPrecision(18, 2);
            });

            builder.Entity<Models.Contract>(b =>
            {
                b.HasOne(c => c.User)
                 .WithMany()
                 .HasForeignKey(c => c.UserId)
                 .OnDelete(DeleteBehavior.Cascade);

                b.Property(c => c.HourlyRate)
                 .HasPrecision(18, 2);

                b.Property(c => c.MaximumMonthlyHours)
                 .HasPrecision(8, 2);
            });

            // Seed initial data
            SeedData(builder);
        }

        private static void SeedData(ModelBuilder builder)
        {
            builder.Entity<Department>().HasData(
                new Department { DepartmentId = 1, Name = "Computer Science", Code = "CS", Description = "Computer Science Department" },
                new Department { DepartmentId = 2, Name = "Mathematics", Code = "MATH", Description = "Mathematics Department" },
                new Department { DepartmentId = 3, Name = "Business Administration", Code = "BUS", Description = "Business Administration Department" }
            );

            builder.Entity<SystemSetting>().HasData(
                new SystemSetting { Id = 1, Key = "MaxClaimAmount", Value = "50000", Description = "Maximum allowed claim amount" },
                new SystemSetting { Id = 2, Key = "AutoApproveThreshold", Value = "10000", Description = "Claims below this amount are auto-approved" },
                new SystemSetting { Id = 3, Key = "MaxMonthlyHours", Value = "160", Description = "Maximum hours allowed per month" },
                new SystemSetting { Id = 4, Key = "DefaultHourlyRate", Value = "350", Description = "Default hourly rate for claims" }
            );
        }
    }
}