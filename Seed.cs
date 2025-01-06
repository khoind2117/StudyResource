using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using StudyResource.Data;
using StudyResource.Dictionaries;
using StudyResource.Models;
using StudyResource.Services;
using System.Net.WebSockets;

namespace StudyResource
{
    public class Seed
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly SlugService _slugService;

        public Seed(ApplicationDbContext context,
            UserManager<User> userManager,
            RoleManager<IdentityRole> roleManager,
            SlugService slugService)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
            _slugService = slugService;
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
            // Seed Admin
            if (!_context.Users.Any())
            {
                var adminUser = new User
                {
                    UserName = "admin",
                    FirstName = "Admin",
                    LastName = "Real",
                    Email = "admin@gmail.com",
                    EmailConfirmed = true,
                    NormalizedUserName = "ADMIN@GMAIL.COM",
                    NormalizedEmail = "ADMIN@GMAIL.COM",
                    SecurityStamp = Guid.NewGuid().ToString()
                };

                var password = "Admin@123";
                var result = await _userManager.CreateAsync(adminUser, password);
                if (result.Succeeded)
                {
                    await _userManager.AddToRoleAsync(adminUser, "Admin");
                }

                for (int i = 1; i <= 5; i++)
                {
                    var user = new User
                    {
                        UserName = $"user{i}",
                        FirstName = $"Nguyễn Văn",
                        LastName = $"{(char)('A' + i - 1)}",
                        Email = $"user{i}@gmail.com",
                        EmailConfirmed = true,
                        NormalizedUserName = $"USER{i}@GMAIL.COM",
                        NormalizedEmail = $"USER{i}@GMAIL.COM",
                        SecurityStamp = Guid.NewGuid().ToString()
                    };

                    var userPassword = $"User@123";
                    var userResult = await _userManager.CreateAsync(user, userPassword);
                    if (userResult.Succeeded)
                    {
                        await _userManager.AddToRoleAsync(user, "User");
                    }
                }

                await _context.SaveChangesAsync();
            }
            #endregion

            #region Grade
            if (!_context.Grades.Any())
            {
                var grades = new List<Grade>
                {
                    new Grade { Name = "Lớp 1", Slug = _slugService.GenerateSlug("Lớp 1") },
                    new Grade { Name = "Lớp 2", Slug = _slugService.GenerateSlug("Lớp 2") },
                    new Grade { Name = "Lớp 3", Slug = _slugService.GenerateSlug("Lớp 3") },
                    new Grade { Name = "Lớp 4", Slug = _slugService.GenerateSlug("Lớp 4") },
                    new Grade { Name = "Lớp 5", Slug = _slugService.GenerateSlug("Lớp 5") },
                    new Grade { Name = "Lớp 6", Slug = _slugService.GenerateSlug("Lớp 6") },
                    new Grade { Name = "Lớp 7", Slug = _slugService.GenerateSlug("Lớp 7") },
                    new Grade { Name = "Lớp 8", Slug = _slugService.GenerateSlug("Lớp 8") },
                    new Grade { Name = "Lớp 9", Slug = _slugService.GenerateSlug("Lớp 9") },
                    new Grade { Name = "Lớp 10", Slug = _slugService.GenerateSlug("Lớp 10") },
                    new Grade { Name = "Lớp 11", Slug = _slugService.GenerateSlug("Lớp 11") },
                    new Grade { Name = "Lớp 12", Slug = _slugService.GenerateSlug("Lớp 12") }
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
                    new Subject { Name = "Toán", Slug = _slugService.GenerateSlug("Toán") },
                    new Subject { Name = "Ngữ văn", Slug = _slugService.GenerateSlug("Ngữ văn") },
                    new Subject { Name = "Vật lý", Slug = _slugService.GenerateSlug("Vật lý") },
                    new Subject { Name = "Hóa học", Slug = _slugService.GenerateSlug("Hóa học") },
                    new Subject { Name = "Sinh học", Slug = _slugService.GenerateSlug("Sinh học") },
                    new Subject { Name = "Lịch sử", Slug = _slugService.GenerateSlug("Lịch sử") },
                    new Subject { Name = "Địa lý", Slug = _slugService.GenerateSlug("Địa lý") },
                    new Subject { Name = "Tiếng Anh", Slug = _slugService.GenerateSlug("Tiếng Anh") },
                    new Subject { Name = "Giáo dục công dân", Slug = _slugService.GenerateSlug("Giáo dục công dân") },
                    new Subject { Name = "Tin học", Slug = _slugService.GenerateSlug("Tin học") },
                    new Subject { Name = "Đạo đức", Slug = _slugService.GenerateSlug("Đạo đức") },
                    new Subject { Name = "Tự nhiên và xã hội", Slug = _slugService.GenerateSlug("Tự nhiên và xã hội") },
                    new Subject { Name = "Công nghệ", Slug = _slugService.GenerateSlug("Công nghệ") },
                    new Subject { Name = "Âm nhạc", Slug = _slugService.GenerateSlug("Âm nhạc") },
                    new Subject { Name = "Mỹ thuật", Slug = _slugService.GenerateSlug("Mỹ thuật") },
                    new Subject { Name = "Giáo dục thể chất", Slug = _slugService.GenerateSlug("Giáo dục thể chất") },
                    new Subject { Name = "Hoạt động trải nghiệm", Slug = _slugService.GenerateSlug("Hoạt động trải nghiệm") },
                    new Subject { Name = "Tiếng việt", Slug = _slugService.GenerateSlug("Tiếng việt") },
                    new Subject { Name = "Khoa học tự nhiên", Slug = _slugService.GenerateSlug("Khoa học tự nhiên") },
                    new Subject { Name = "Lịch sử và địa lý", Slug = _slugService.GenerateSlug("Lịch sử và địa lý") },
                    new Subject { Name = "Giáo dục kinh tế pháp luật", Slug = _slugService.GenerateSlug("Giáo dục kinh tế pháp luật") },
                    new Subject { Name = "Giáo dục quốc phòng", Slug = _slugService.GenerateSlug("Giáo dục quốc phòng") },
                    new Subject { Name = "Tập viết", Slug = _slugService.GenerateSlug("Tập viết") },
                };

                _context.Subjects.AddRange(subjects);
                await _context.SaveChangesAsync();
            };
            #endregion

            #region GradeSubject
            if (!_context.GradeSubjects.Any())
            {
                var gradeSubjects = new List<GradeSubject>();

                foreach (var gradeEntry in SubjectsByGrade.GradeSubjects)
                {
                    int grade = gradeEntry.Key;
                    var subjects = gradeEntry.Value;

                    foreach (var subject in subjects)
                    {
                        string subjectName = subject.Name;
                        int subjectId = subject.SubjectId;

                        gradeSubjects.Add(new GradeSubject
                        {
                            Name = $"{subjectName} lớp {grade}",
                            Slug = _slugService.GenerateSlug($"{subjectName} lớp {grade}"),
                            GradeId = grade,
                            SubjectId = subjectId
                        });
                    }
                }

                _context.GradeSubjects.AddRange(gradeSubjects);
                await _context.SaveChangesAsync();
            }
            #endregion

            #region DocumentType
            if (!_context.DocumentTypes.Any())
            {
                var documentTypes = new List<DocumentType>
                {
                    new DocumentType
                    {
                        Name = "Sách giáo khoa",
                        Slug = _slugService.GenerateSlug("Sách giáo khoa"),
                    },
                    new DocumentType
                    {
                        Name = "Sách bài tập",
                        Slug = _slugService.GenerateSlug("Sách bài tập"),
                    },
                    new DocumentType
                    {
                        Name = "Sách giáo viên",
                        Slug = _slugService.GenerateSlug("Sách giáo viên"),
                    },
                    new DocumentType
                    {
                        Name = "Tài liệu tham khảo",
                        Slug = _slugService.GenerateSlug("Tài liệu tham khảo"),
                    }
                };

                _context.DocumentTypes.AddRange(documentTypes);
                await _context.SaveChangesAsync();
            };
            #endregion

            #region Set
            if (!_context.Sets.Any())
            {
                var sets = new List<Set>
                {
                    new Set
                    {
                        Name = "Cánh Diều",
                        Slug = _slugService.GenerateSlug("Cánh Diều"),
                    },
                    new Set
                    {
                        Name = "Chân Trời Sáng Tạo",
                        Slug = _slugService.GenerateSlug("Chân Trời Sáng Tạo"),
                    },
                    new Set
                    {
                        Name = "Kết Nối Tri Thức",
                        Slug = _slugService.GenerateSlug("Kết Nối Tri Thức"),
                    },
                };

                _context.Sets.AddRange(sets);
                await _context.SaveChangesAsync();
            }
            #endregion

            //    #region DownloadHistory 2nd
            //    if (!_context.DownloadHistories.Any())
            //    {
            //        var random = new Random();
            //        var downloadHistory = new List<DownloadHistory>();
            //        var startDate = new DateTime(2025, 1, 1);
            //        var endDate = new DateTime(2025, 1, 5);
            //        var totalMinutes = (int)(endDate - startDate).TotalMinutes;

            //        var userRole = await _context.Roles
            //            .FirstOrDefaultAsync(r => r.Name == "User");

            //        if (userRole != null)
            //        {
            //            var userIds = await _context.UserRoles
            //                .Where(ur => ur.RoleId == userRole.Id)
            //                .Select(ur => ur.UserId)
            //                .ToListAsync();

            //            for (int i = 0; i < 1000; i++)
            //            {
            //                var randomMinutes = random.Next(0, totalMinutes);
            //                var randomDate = startDate.AddMinutes(randomMinutes);

            //                var randomUserId = userIds[random.Next(userIds.Count)];

            //                var documentId = random.Next(1, 445);

            //                var document = await _context.Documents
            //                    .FirstOrDefaultAsync(d => d.Id == documentId);

            //                if (document != null)
            //                {
            //                    document.Views += 3;
            //                    document.Downloads += 1;
            //                    _context.Documents.Update(document);
            //                }

            //                downloadHistory.Add(new DownloadHistory
            //                {
            //                    DownloadDate = randomDate,
            //                    UserId = randomUserId,
            //                    DocumentId = documentId
            //                });
            //            }

            //            _context.DownloadHistories.AddRange(downloadHistory);
            //            await _context.SaveChangesAsync();
            //        }
            //    }
            //    #endregion

            //    #region UserComments 2nd
            //    if (!_context.UserComments.Any())
            //    {
            //        var random = new Random();
            //        var comments = new List<UserComment>();
            //        var startDate = new DateTime(2025, 1, 1);
            //        var endDate = new DateTime(2025, 1, 5);
            //        var totalMinutes = (int)(endDate - startDate).TotalMinutes;

            //        var userRole = await _context.Roles
            //            .FirstOrDefaultAsync(r => r.Name == "User");

            //        if (userRole != null)
            //        {
            //            var userIds = await _context.UserRoles
            //                .Where(ur => ur.RoleId == userRole.Id)
            //                .Select(ur => ur.UserId)
            //                .ToListAsync();

            //            var documentIds = await _context.Documents.Select(d => d.Id).ToListAsync();

            //            var commentTemplates = new List<string>
            //            {
            //                "Tài liệu này rất hữu ích, tôi đã học được nhiều điều mới.",
            //                "Không thích tài liệu này, mong muốn có nhiều thông tin hơn.",
            //                "Tài liệu này cung cấp thông tin rất đầy đủ.",
            //                "Mong rằng có thêm các tài liệu tương tự.",
            //                "Rất tuyệt vời! Tài liệu này đã giúp tôi hoàn thành công việc.",
            //                "Chất lượng tài liệu khá tốt, nhưng có thể cải thiện thêm.",
            //                "Rất hay! Cảm ơn vì đã chia sẻ tài liệu này.",
            //                "Tài liệu này khá dễ hiểu và dễ áp dụng.",
            //                "Một tài liệu tuyệt vời, tôi sẽ giới thiệu cho bạn bè.",
            //                "Tài liệu này hơi dài nhưng rất hữu ích."
            //            };

            //            for (int i = 0; i < 1000; i++) 
            //            {
            //                var randomMinutes = random.Next(0, totalMinutes);
            //                var randomDate = startDate.AddMinutes(randomMinutes);

            //                var randomUserId = userIds[random.Next(userIds.Count)];
            //                var randomDocumentId = documentIds[random.Next(documentIds.Count)];

            //                var randomRating = random.Next(1, 6);
            //                var randomComment = commentTemplates[random.Next(commentTemplates.Count)];

            //                comments.Add(new UserComment
            //                {
            //                    UserId = randomUserId,
            //                    Comment = randomComment,
            //                    Rating = randomRating,
            //                    CommentDate = randomDate,
            //                    DocumentId = randomDocumentId
            //                });
            //            }

            //            _context.UserComments.AddRange(comments);
            //            await _context.SaveChangesAsync();
            //        }
            //    }
            //    #endregion
            //}
        }
    }
}
