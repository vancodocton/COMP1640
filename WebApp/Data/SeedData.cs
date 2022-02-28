using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using WebApp.Models;

namespace WebApp.Data
{
    public class SeedData
    {
        public async static Task Initializer(IServiceProvider serviceProvider, IConfiguration configuration)
        {
            await SeedRoleData(serviceProvider.GetRequiredService<RoleManager<IdentityRole>>());

            await SeedDefaultAccountData(serviceProvider.GetRequiredService<UserManager<ApplicationUser>>());

            await SeedDepartmentData(serviceProvider.GetRequiredService<ApplicationDbContext>());
        }

        private static async Task SeedRoleData(RoleManager<IdentityRole> roleManager)
        {
            var roles = new List<string>()
            {
                Role.Admin, Role.Manager, Role.Coordinator, Role.Staff
            };

            foreach (var role in roles)
            {
                if (await roleManager.RoleExistsAsync(role) == false)
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }
        }

        private static async Task SeedDefaultAccountData(UserManager<ApplicationUser> userManager)
        {
            var admin = new ApplicationUser
            {
                UserName = "admin@g.c",
                Email = "admin@g.c",
                EmailConfirmed = true,
            };
            var manager = new ApplicationUser
            {
                UserName = "manager@g.c",
                Email = "manager@g.c",
                EmailConfirmed = true,
            };
            string password = "asd@12E";

            // Seed admin account
            if (await userManager.FindByEmailAsync(admin.Email) == null)
            {
                var result = await userManager.CreateAsync(admin, password);

                if (result.Succeeded)
                    await userManager.AddToRoleAsync(admin, Role.Admin);
            }

            // Seed manager account
            if (await userManager.FindByEmailAsync(manager.Email) == null)
            {
                var result = await userManager.CreateAsync(manager, password);

                if (result.Succeeded)
                    await userManager.AddToRoleAsync(manager, Role.Manager);
            }
        }

        private static async Task SeedDepartmentData(ApplicationDbContext context)
        {
            var departments = new List<Department>()
            {
                new Department() { Name="Academic" },
                new Department() { Name="Support" },
            };

            foreach (var department in departments)
            {
                if (!await context.Department.AnyAsync(d => d.Name == department.Name))
                    await context.Department.AddAsync(department);
            }

            await context.SaveChangesAsync();
        }
    }
}
