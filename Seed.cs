﻿using Microsoft.AspNetCore.Identity;
using StudyResource.Data;
using StudyResource.Models;
using StudyResource.Services;

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
                    new Subject { Name = "Âm nhạc", Slug = _slugService.GenerateSlug("Âm nhạc") }
                };

                _context.Subjects.AddRange(subjects);
                await _context.SaveChangesAsync();
            };
            #endregion

            #region GradeSubject
            //if (!_context.GradeSubjects.Any())
            //{
            //    var gradeSubjects = new List<GradeSubject>();

            //    for (int grade = 1; grade <= 12; grade++)
            //    {
            //        gradeSubjects.Add(new GradeSubject { Name = $"Toán lớp {grade}", Slug = _slugService.GenerateSlug($"Toán lớp {grade}"), GradeId = grade, SubjectId = 1 });
            //        gradeSubjects.Add(new GradeSubject { Name = $"Ngữ văn lớp {grade}", Slug = _slugService.GenerateSlug($"Ngữ văn lớp {grade}"), GradeId = grade, SubjectId = 2 });
            //        gradeSubjects.Add(new GradeSubject { Name = $"Vật lý lớp {grade}", Slug = _slugService.GenerateSlug($"Vật lý lớp {grade}"), GradeId = grade, SubjectId = 3 });
            //        gradeSubjects.Add(new GradeSubject { Name = $"Hóa học lớp {grade}", Slug = _slugService.GenerateSlug($"Hóa học lớp {grade}"), GradeId = grade, SubjectId = 4 });
            //        gradeSubjects.Add(new GradeSubject { Name = $"Sinh học lớp {grade}", Slug = _slugService.GenerateSlug($"Sinh học lớp {grade}"), GradeId = grade, SubjectId = 5 });
            //        gradeSubjects.Add(new GradeSubject { Name = $"Lịch sử lớp {grade}", Slug = _slugService.GenerateSlug($"Lịch sử lớp {grade}"), GradeId = grade, SubjectId = 6 });
            //        gradeSubjects.Add(new GradeSubject { Name = $"Địa lý lớp {grade}", Slug = _slugService.GenerateSlug($"Địa lý lớp {grade}"), GradeId = grade, SubjectId = 7 });
            //        gradeSubjects.Add(new GradeSubject { Name = $"Tiếng Anh lớp {grade}", Slug = _slugService.GenerateSlug($"Tiếng Anh lớp {grade}"), GradeId = grade, SubjectId = 8 });
            //        gradeSubjects.Add(new GradeSubject { Name = $"Giáo dục công dân lớp {grade}", Slug = _slugService.GenerateSlug($"Giáo dục công dân lớp {grade}"), GradeId = grade, SubjectId = 9 });
            //        gradeSubjects.Add(new GradeSubject { Name = $"Tin học lớp {grade}", Slug = _slugService.GenerateSlug($"Tin học lớp {grade}"), GradeId = grade, SubjectId = 10 });
            //        gradeSubjects.Add(new GradeSubject { Name = $"Đạo đức lớp {grade}", Slug = _slugService.GenerateSlug($"Đạo đức lớp {grade}"), GradeId = grade, SubjectId = 11 });
            //        gradeSubjects.Add(new GradeSubject { Name = $"Tự nhiên và xã hội lớp {grade}", Slug = _slugService.GenerateSlug($"Tự nhiên và xã hội lớp {grade}"), GradeId = grade, SubjectId = 12 });
            //        gradeSubjects.Add(new GradeSubject { Name = $"Công nghệ lớp {grade}", Slug = _slugService.GenerateSlug($"Công nghệ lớp {grade}"), GradeId = grade, SubjectId = 13 });
            //        gradeSubjects.Add(new GradeSubject { Name = $"Âm nhạc lớp {grade}", Slug = _slugService.GenerateSlug($"Âm nhạc lớp {grade}"), GradeId = grade, SubjectId = 14 });
            //    }


            //    _context.GradeSubjects.AddRange(gradesubjects);
            //    await _context.SaveChangesAsync();
            //};
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
                        Name = "Sách bổ trợ",
                        Slug = _slugService.GenerateSlug("Sách bổ trợ"),
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

            #region Document
            if (!_context.Documents.Any())
            {
                var documents = new List<Document>
                {
                    new Document
                    {
                        Title = "Đề thi giữa học kỳ 1 môn lịch sử lớp 10",
                        Slug = _slugService.GenerateSlug("Đề thi giữa học kỳ 1 môn lịch sử lớp 10"),
                        Description = "Đề thi giữa học kỳ 1 môn lịch sử lớp 10",
                        Views = 10,
                        Downloads = 10,
                        GoogleDriveId = "1jMFlkFeVAs0z8Cdapt3vQ3uE2T0tP9Hd",
                        UploadDate = DateTime.Now,
                        GradeSubjectId = 6,
                        DocumentTypeId = 4
                    },
                    new Document
                    {
                        Title = "Đề thi học kì 2 môn địa lí lớp 10 Nghệ An",
                        Slug = _slugService.GenerateSlug("Đề thi học kì 2 môn địa lí lớp 10 Nghệ An"),
                        Description = "Đề thi học kì 2 môn địa lí lớp 10 Nghệ An",
                        Views = 20,
                        Downloads = 20,
                        GoogleDriveId = "1aJEvMQ5sLGP8sObfI2AjQRo1sepNPc2p",
                        UploadDate = DateTime.Now,
                        GradeSubjectId = 7,
                        DocumentTypeId = 4
                    },
                    new Document
                    {
                        Title = "Ôn tập giữa kì 1 toán 10 năm 2024-2025 Trường thpt Trần Phú Hà Nội",
                        Slug = _slugService.GenerateSlug("Ôn tập giữa kì 1 toán 10 năm 2024-2025 Trường thpt Trần Phú Hà Nội"),
                        Description = "Ôn tập giữa kì 1 toán 10 năm 2024-2025 Trường thpt Trần Phú Hà Nội",
                        Views = 30,
                        Downloads = 30,
                        GoogleDriveId = "1tYiA7Bpqhn5t6bThJm4zHdrZVzACYVOb",
                        UploadDate = DateTime.Now,
                        GradeSubjectId = 1,
                        DocumentTypeId = 4
                    },
                    new Document
                    {
                        Title = "Đề thi hk2 Toán 10 năm 2018-2019 Trường Phạm Văn Đồng Quảng Ngãi",
                        Slug = _slugService.GenerateSlug("Đề thi hk2 Toán 10 năm 2018-2019 Trường Phạm Văn Đồng Quảng Ngãi"),
                        Description = "Đề thi hk2 Toán 10 năm 2018-2019 Trường Phạm Văn Đồng Quảng Ngãi",
                        Views = 40,
                        Downloads = 40,
                        GoogleDriveId = "1ceT4lDyQEBzi7GD2MSwvB1YKh4cWvqif",
                        UploadDate = DateTime.Now,
                        GradeSubjectId = 1,
                        DocumentTypeId = 4
                    },
                    new Document
                    {
                        Title = "Đề thi học kỳ 2 năm 2018-2019 Trường Lê Quý Đôn Quảng Ninh",
                        Slug = _slugService.GenerateSlug("Đề thi học kỳ 2 năm 2018-2019 Trường Lê Quý Đôn Quảng Ninh"),
                        Description = "Đề thi học kỳ 2 năm 2018-2019 Trường Lê Quý Đôn Quảng Ninh",
                        Views = 50,
                        Downloads = 50,
                        GoogleDriveId = "1TW7U8rV85smmQwsfVB_ZRGyNWqZRm5UD",
                        UploadDate = DateTime.Now,
                        GradeSubjectId = 1,
                        DocumentTypeId = 4
                    },
                    new Document
                    {
                        Title = "Chuyên đề hàm số bậc hai và đồ thị toán 10",
                        Slug = _slugService.GenerateSlug("Chuyên đề hàm số bậc hai và đồ thị toán 10"),
                        Description = "Chuyên đề hàm số bậc hai và đồ thị toán 10",
                        Views = 60,
                        Downloads = 60,
                        GoogleDriveId = "1KePjpoNhi94AuuC01nD-2cGrs5p-mzz2",
                        UploadDate = DateTime.Now,
                        GradeSubjectId = 1,
                        DocumentTypeId = 3
                    },
                    new Document
                    {
                        Title = "Chuyên đề vecto Toán 10",
                        Slug = _slugService.GenerateSlug("Chuyên đề vecto Toán 10"),
                        Description = "Chuyên đề vecto Toán 10",
                        Views = 70,
                        Downloads = 70,
                        GoogleDriveId = "1HtYLTkH2aBUbjsw43EHy7j34C3JDvfT3",
                        UploadDate = DateTime.Now,
                        GradeSubjectId = 1,
                        DocumentTypeId = 3
                    },
                    new Document
                    {
                        Title = "Bất phương trình và phương trình",
                        Slug = _slugService.GenerateSlug("Bất phương trình và phương trình"),
                        Description = "Bất phương trình và phương trình",
                        Views = 80,
                        Downloads = 80,
                        GoogleDriveId = "1Zkjt_SPrgrXLAFDyjwzdtGm8cjZqxgxX",
                        UploadDate = DateTime.Now,
                        GradeSubjectId = 1,
                        DocumentTypeId = 3
                    },
                    new Document
                    {
                        Title = "Toán 10_Bài 1_Chương 5: Quy tắc cộng, Quy tắc nhân, Sơ đồ hình cây",
                        Slug = _slugService.GenerateSlug("Toán 10_Bài 1_Chương 5: Quy tắc cộng, Quy tắc nhân, Sơ đồ hình cây"),
                        Description = "Toán 10_Bài 1_Chương 5: Quy tắc cộng, Quy tắc nhân, Sơ đồ hình cây",
                        Views = 90,
                        Downloads = 90,
                        GoogleDriveId = "1r5oQK2S9BH60Xu9GrSmZhpVv6Ps-9GWd",
                        UploadDate = DateTime.Now,
                        GradeSubjectId = 1,
                        DocumentTypeId = 3
                    },
                    new Document
                    {
                        Title = "Tuần 2: Bài 2: Tập hợp, các phép toán trên tập hợp (Tiết 1)",
                        Slug = _slugService.GenerateSlug("Tuần 2: Bài 2: Tập hợp, các phép toán trên tập hợp (Tiết 1)"),
                        Description = "Tuần 2: Bài 2: Tập hợp, các phép toán trên tập hợp (Tiết 1)",
                        Views = 100,
                        Downloads = 100,
                        GoogleDriveId = "1y60mzo-GvlWayWs_Ay2ferGO3b3FD5U_",
                        UploadDate = DateTime.Now,
                        GradeSubjectId = 1,
                        DocumentTypeId = 2
                    }
                };

                _context.Documents.AddRange(documents);
                await _context.SaveChangesAsync();
            }
            #endregion
        }
    }
}
