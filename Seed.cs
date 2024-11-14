using Microsoft.AspNetCore.Identity;
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
                    UserName = "admin@gmail.com",
                    FirstName = "Admin",
                    LastName = "TLTN",
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
                        Name = "Bài giảng",
                        Slug = _slugService.GenerateSlug("Bài giảng"),
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

            #region Document
            //if (!_context.Documents.Any())
            //{
            //    var documents = new List<Document>
            //    {
            //        new Document
            //        {
            //            Title = "Sách giáo khoa Vật Lý 11 Cánh Diều",
            //            Slug = _slugService.GenerateSlug("Sách giáo khoa Vật Lý 11 Cánh Diều"),
            //            Description = "Sách giáo khoa Vật Lý 11 Cánh Diều",
            //            Views = 10,
            //            Downloads = 10,
            //            GoogleDriveId = "1jMFlkFeVAs0z8Cdapt3vQ3uE2T0tP9Hd",
            //            UploadDate = DateTime.Now,
            //            GradeSubjectId = 107,
            //            DocumentTypeId = 1,
            //            SetId = 1
            //        },
            //        new Document
            //        {
            //            Title = "Ngữ Văn Lớp 12 Tập Một",
            //            Slug = _slugService.GenerateSlug("Ngữ Văn Lớp 12 Tập Một"),
            //            Description = "Ngữ Văn Lớp 12 Tập Một",
            //            Views = 20,
            //            Downloads = 20,
            //            GoogleDriveId = "18FAkSFzbOuh9W1vl5H9qJyL4Rlunv3tF",
            //            UploadDate = DateTime.Now,
            //            GradeSubjectId = 718,
            //            DocumentTypeId = 1,
            //            SetId = 2
            //        },
            //        new Document
            //        {
            //            Title = "Ngữ Văn Lớp 12 Tập Hai",
            //            Slug = _slugService.GenerateSlug("Ngữ Văn Lớp 12 Tập Hai"),
            //            Description = "Ngữ Văn Lớp 12 Tập Hai",
            //            Views = 30,
            //            Downloads = 30,
            //            GoogleDriveId = "1B7r44HkqgemeRaC2VIO9XhJLSTvjDe2H",
            //            UploadDate = DateTime.Now,
            //            GradeSubjectId = 118,
            //            DocumentTypeId = 1,
            //            SetId = 3
            //        },
            //        new Document
            //        {
            //            Title = "Sách giáo khoa Giải tích Lớp 12",
            //            Slug = _slugService.GenerateSlug("Sách giáo khoa Giải tích Lớp 12"),
            //            Description = "Sách giáo khoa Giải tích Lớp 12",
            //            Views = 40,
            //            Downloads = 40,
            //            GoogleDriveId = "1tOEv8fNWQwwxtzfLua13papq-8OQ_Q30",
            //            UploadDate = DateTime.Now,
            //            GradeSubjectId = 117,
            //            DocumentTypeId = 1,
            //            SetId = 1
            //        },
            //        new Document
            //        {
            //            Title = "Công nghệ Lớp 12",
            //            Slug = _slugService.GenerateSlug("Công nghệ Lớp 12"),
            //            Description = "Công nghệ Lớp 12",
            //            Views = 50,
            //            Downloads = 50,
            //            GoogleDriveId = "1Zy2RHRuQOH_kt8fLhuGlclP00xBNSxVe",
            //            UploadDate = DateTime.Now,
            //            GradeSubjectId = 127,
            //            DocumentTypeId = 1,
            //            SetId = 1
            //        },
            //    };

            //    _context.Documents.AddRange(documents);
            //    await _context.SaveChangesAsync();
            //}
            #endregion

            #region DownloadHistory
            if (!_context.DownloadHistories.Any())
            {
                var random = new Random();
                var downloadHistory = new List<DownloadHistory>();
                var startDate = new DateTime(2024, 11, 7);
                var endDate = new DateTime(2024, 11, 14);
                var totalMinutes = (int)(endDate - startDate).TotalMinutes;

                for (int i = 0; i < 100; i++)
                {
                    var randomMinutes = random.Next(0, totalMinutes);
                    var randomDate = startDate.AddMinutes(randomMinutes);

                    var documentId = random.Next(1, 51);
                    var userId = "c11527ac-24a4-4595-8372-c392fde740d8";

                    downloadHistory.Add(new DownloadHistory
                    {
                        DownloadDate = randomDate,
                        UserId = userId,
                        DocumentId = documentId
                    });
                }

                _context.DownloadHistories.AddRange(downloadHistory);
                await _context.SaveChangesAsync();
            }
            #endregion
        }
    }
}
