using System.Security.Claims;
using Microsoft.AspNetCore.Identity;

namespace KXlsxConverterAPI.Models;

public static class SeedData
{
    public static async Task Initialize(IServiceProvider serviceProvider, UserManager<EmployeeUser> userManager, RoleManager<IdentityRole> roleManager, IConfiguration configuration)
    {
        var roleNames = new[] { "Admin", "User" };
        var rolesExist = roleNames.All(role => roleManager.Roles.Any(r => r.Name == role));
        var storeNumbers = new[] { "000-000", "016-549" };
        var storeClaims = storeNumbers.Select(x => new Claim("StoreNumber", x));

        if (!rolesExist)
        {
            foreach (var roleName in roleNames)
            {
                var role = new IdentityRole(roleName);
                await roleManager.CreateAsync(role);
            }
        }

        if (!string.IsNullOrEmpty(configuration["USERNAME"]))
        {
            var adminUser = await userManager.FindByNameAsync(configuration["USERNAME"]!);

            if (adminUser == null)
            {
                adminUser = new EmployeeUser
                {
                    UserName = configuration["USERNAME"],
                };

                var result = await userManager.CreateAsync(adminUser, configuration["PASSWORD"]!);

                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(adminUser, "Admin");
                    
                    await userManager.AddClaimsAsync(adminUser, storeClaims);
                    
                }
            }
        }

    }
}