using Microsoft.AspNetCore.Identity;
using StudyResource.Data;
using StudyResource.Models;

namespace StudyResource
{
    public class Seed
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public Seed(ApplicationDbContext context, UserManager<User> userManager, RoleManager<IdentityRole> roleManager)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
        }


        public async Task SeedApplicationDbContextAsync()
        {
            #region AspNetRoles
            if (!_context.Roles.Any())
            {
                var roles = new List<IdentityRole>
                {
                    new IdentityRole
                    {
                        Id = Guid.NewGuid().ToString(),
                        Name = "Admin",
                        NormalizedName = "ADMIN",
                        ConcurrencyStamp = Guid.NewGuid().ToString()
                    },
                    new IdentityRole
                    {
                        Id = Guid.NewGuid().ToString(),
                        Name = "User",
                        NormalizedName = "USER",
                        ConcurrencyStamp = Guid.NewGuid().ToString()
                    }
                };

                _context.Roles.AddRange(roles);
                await _context.SaveChangesAsync();
            }
            #endregion

            #region AspNetUsers
            // Seed người dùng quản trị viên (Admin)
            if (!_context.Users.Any())
            {
                var adminUser = new User
                {
                    UserName = "admin@gmail.com",
                    FirstName = "Admin",
                    LastName = "TLTN",
                    Email = "admin@gmail.com",
                    EmailConfirmed = true,
                    NormalizedUserName = "ADMIN@GMAIL.COM",
                    NormalizedEmail = "ADMIN@GMAIL.COM",
                    SecurityStamp = Guid.NewGuid().ToString()
                };

                // Tạo người dùng với mật khẩu
                var password = "Admin@123";
                var result = await _userManager.CreateAsync(adminUser, password);
                if (result.Succeeded)
                {
                    await _userManager.AddToRoleAsync(adminUser, "Admin");
                }
                
                await _context.SaveChangesAsync();
            }
            #endregion
        }
    }
}
