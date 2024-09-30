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

            #region Grade
            if (!_context.Grades.Any())
            {
                var grades = new List<Grade>
                {
                    new Grade
                    {
                        Id = 1,
                        Name = "Lớp 1",
                        Slug = "lop-1"
                    },
                    new Grade
                    {
                        Id = 2,
                        Name = "Lớp 2",
                        Slug = "lop-2"
                    },
                    new Grade
                    {
                        Id = 3,
                        Name = "Lớp 3",
                        Slug = "lop-3"
                    },
                    new Grade
                    {
                        Id = 4,
                        Name = "Lớp 4",
                        Slug = "lop-4"
                    },
                    new Grade
                    {
                        Id = 5,
                        Name = "Lớp 5",
                        Slug = "lop-5"
                    },
                    new Grade
                    {
                        Id = 6,
                        Name = "Lớp 6",
                        Slug = "lop-6"
                    },
                    new Grade
                    {
                        Id =7,
                        Name = "Lớp 7",
                        Slug = "lop-7"
                    },
                    new Grade
                    {
                        Id = 8,
                        Name = "Lớp 8",
                        Slug = "lop-8"
                    },
                    new Grade
                    {
                        Id = 9,
                        Name = "Lớp 9",
                        Slug = "lop-9"
                    },
                    new Grade
                    {
                        Id = 10,
                        Name = "Lớp 10",
                        Slug = "lop-10"
                    },
                    new Grade
                    {
                        Id = 11,
                        Name = "Lớp 11",
                        Slug = "lop-11"
                    },
                    new Grade
                    {
                        Id = 12,
                        Name = "Lớp 12",
                        Slug = "lop-12"
                    }
                };
                
                _context.Grades.AddRange(grades);
                await _context.SaveChangesAsync();
            };
            #endregion

            #region Subject
            if (!_context.Subjects.Any())
            {
                var subjects = new List<Subject>
                {
                    new Subject
                    {
                        Id = 1,
                        Name = "Toán",
                        Slug = "toan"
                    },
                    new Subject
                    {
                        Id = 2,
                        Name = "Ngữ văn",
                        Slug = "ngu-van"
                    },
                    new Subject
                    {
                        Id = 3,
                        Name = "Vật lý",
                        Slug = "vat-ly"
                    },
                    new Subject
                    {
                        Id = 4,
                        Name = "Hóa học",
                        Slug = "hoa-hoc"
                    },
                    new Subject
                    {
                        Id = 5,
                        Name = "Sinh học",
                        Slug = "sinh-hoc"
                    },
                    new Subject
                    {
                        Id = 6,
                        Name = "Lịch sử",
                        Slug = "lich-su"
                    },
                    new Subject
                    {
                        Id = 7,
                        Name = "Địa lý",
                        Slug = "dia-ly"
                    },
                    new Subject
                    {
                        Id = 8,
                        Name = "Tiếng Anh",
                        Slug = "tieng-anh"
                    },
                    new Subject
                    {
                        Id = 9,
                        Name = "Giáo dục công dân",
                        Slug = "giao-duc-cong-dan"
                    },
                    new Subject
                    {
                        Id = 10,
                        Name = "Tin học",
                        Slug = "tin-hoc"
                    }
                };

                _context.Subjects.AddRange(subjects);
                await _context.SaveChangesAsync();
            };
            #endregion
        }
    }
}
