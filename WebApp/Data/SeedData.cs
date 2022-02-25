using Microsoft.AspNetCore.Identity;
using WebApp.Models;

namespace WebApp.Data
{
    public class SeedData
    {
        public async static Task Initializer(IServiceProvider serviceProvider, IConfiguration configuration)
        {
            using (var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>())
            {
                if (await roleManager.RoleExistsAsync(Role.Admin) == false)
                {
                    await roleManager.CreateAsync(new IdentityRole(Role.Admin));
                }

                if (await roleManager.RoleExistsAsync(Role.Manager) == false)
                {
                    await roleManager.CreateAsync(new IdentityRole(Role.Manager));
                }

                if (await roleManager.RoleExistsAsync(Role.Coordinator) == false)
                {
                    await roleManager.CreateAsync(new IdentityRole(Role.Coordinator));
                }

                if (await roleManager.RoleExistsAsync(Role.Staff) == false)
                {
                    await roleManager.CreateAsync(new IdentityRole(Role.Staff));
                }
            }
            using (var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>())
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

                if (await userManager.FindByEmailAsync(admin.Email) == null)
                {
                    var result = await userManager.CreateAsync(admin, password);

                    if (result.Succeeded)
                        await userManager.AddToRoleAsync(admin, Role.Admin);
                }

                if (await userManager.FindByEmailAsync(manager.Email) == null)
                {
                    var result = await userManager.CreateAsync(manager, password);

                    if (result.Succeeded)
                        await userManager.AddToRoleAsync(manager, Role.Manager);
                }
            }
        }
    }
}
