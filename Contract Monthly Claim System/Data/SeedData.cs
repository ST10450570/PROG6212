using Microsoft.AspNetCore.Identity;
using Contract_Monthly_Claim_System.Models;

namespace Contract_Monthly_Claim_System.Data
{
    public static class SeedData
    {
        public static async Task InitializeAsync(ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            RoleManager<ApplicationRole> roleManager)
        {
            // Ensure departments are seeded first
            if (!context.Departments.Any())
            {
                context.Departments.AddRange(
                    new Department { DepartmentId = 1, Name = "Computer Science", Code = "CS", Description = "Computer Science Department" },
                    new Department { DepartmentId = 2, Name = "Mathematics", Code = "MATH", Description = "Mathematics Department" },
                    new Department { DepartmentId = 3, Name = "Business Administration", Code = "BUS", Description = "Business Administration Department" }
                );
                await context.SaveChangesAsync();
            }

            // Create roles
            string[] roleNames = { "Administrator", "Manager", "Coordinator", "Lecturer" };

            foreach (var roleName in roleNames)
            {
                var roleExist = await roleManager.RoleExistsAsync(roleName);
                if (!roleExist)
                {
                    await roleManager.CreateAsync(new ApplicationRole
                    {
                        Name = roleName,
                        Description = $"{roleName} role",
                        CreatedDate = DateTime.Now
                    });
                }
            }

            // Create admin user
            var adminUser = await userManager.FindByEmailAsync("admin@university.com");
            if (adminUser == null)
            {
                adminUser = new ApplicationUser
                {
                    UserName = "admin@university.com",
                    Email = "admin@university.com",
                    FirstName = "System",
                    LastName = "Administrator",
                    EmailConfirmed = true,
                    IsActive = true,
                    CreatedDate = DateTime.Now
                };

                var result = await userManager.CreateAsync(adminUser, "Admin123!");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(adminUser, "Administrator");
                }
            }

            // Create sample coordinator
            var coordinatorUser = await userManager.FindByEmailAsync("coordinator@university.com");
            if (coordinatorUser == null)
            {
                coordinatorUser = new ApplicationUser
                {
                    UserName = "coordinator@university.com",
                    Email = "coordinator@university.com",
                    FirstName = "Sarah",
                    LastName = "Johnson",
                    EmailConfirmed = true,
                    IsActive = true,
                    DepartmentId = 1, // Computer Science
                    CreatedDate = DateTime.Now
                };

                var result = await userManager.CreateAsync(coordinatorUser, "Coordinator123!");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(coordinatorUser, "Coordinator");
                }
            }

            // Create sample manager
            var managerUser = await userManager.FindByEmailAsync("manager@university.com");
            if (managerUser == null)
            {
                managerUser = new ApplicationUser
                {
                    UserName = "manager@university.com",
                    Email = "manager@university.com",
                    FirstName = "Michael",
                    LastName = "Brown",
                    EmailConfirmed = true,
                    IsActive = true,
                    DepartmentId = 1, // Computer Science
                    CreatedDate = DateTime.Now
                };

                var result = await userManager.CreateAsync(managerUser, "Manager123!");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(managerUser, "Manager");
                }
            }
            
            // Create sample lecturer
            var lecturerUser = await userManager.FindByEmailAsync("lecturer@university.com");
            if (lecturerUser == null)
            {
                lecturerUser = new ApplicationUser
                {
                    UserName = "lecturer@university.com",
                    Email = "lecturer@university.com",
                    FirstName = "David",
                    LastName = "Wilson",
                    EmployeeNumber = "EMP001",
                    EmailConfirmed = true,
                    IsActive = true,
                    DepartmentId = 1, // Computer Science
                    CreatedDate = DateTime.Now
                };

                var result = await userManager.CreateAsync(lecturerUser, "Lecturer123!");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(lecturerUser, "Lecturer");
                }
            }

            // Seed system settings
            if (!context.SystemSettings.Any())
            {
                context.SystemSettings.AddRange(
                    new SystemSetting { Key = "MaxClaimAmount", Value = "50000", Description = "Maximum allowed claim amount" },
                    new SystemSetting { Key = "AutoApproveThreshold", Value = "10000", Description = "Claims below this amount are auto-approved" },
                    new SystemSetting { Key = "MaxMonthlyHours", Value = "160", Description = "Maximum hours allowed per month" },
                    new SystemSetting { Key = "DefaultHourlyRate", Value = "350", Description = "Default hourly rate for claims" }
                );
                await context.SaveChangesAsync();
            }
        }
    }
}