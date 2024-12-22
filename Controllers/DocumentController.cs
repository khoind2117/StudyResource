using CsvHelper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using StudyResource.Data;
using StudyResource.Models;
using StudyResource.Services;
using StudyResource.ViewModels;
using StudyResource.ViewModels.Document;
using System;
using System.Globalization;
using System.Linq;
using System.Security.Claims;
using System.Text.Json;
using System.Threading.Tasks;
using X.PagedList.Extensions;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
using static Microsoft.Extensions.Logging.EventSource.LoggingEventSource;

namespace StudyResource.Controllers
{
    [Route("tai-lieu")]
    public class DocumentController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<User> _userManager;
        private readonly GoogleDriveService _googleDriveService;
        private readonly SlugService _slugService;

        public DocumentController(ApplicationDbContext context,
            UserManager<User> userManager,
            GoogleDriveService googleDriveService,
            SlugService slugService)
        {
            _context = context;
            _userManager = userManager;
            _googleDriveService = googleDriveService;
            _slugService = slugService;
        }

        [HttpGet]
        [Route("ebook/{id}")]
        public async Task<IActionResult> Ebook(int id)
        {
            var document = await _context.Documents.FindAsync(id);
            string fileUrl = Url.Action("DownloadFile", "GoogleDrive", new { fileId = document.GoogleDriveId });
            ViewData["DefaultFileUrl"] = fileUrl;
            ViewData["DocId"] = document.Id;
            return View();
        }

        [HttpGet]
        public IActionResult Index(string searchString, int? gradeId = 1, int? setId = 1)
        {
            var documents = _context.Documents
                .Include(d => d.GradeSubject.Grade)
                .Include(d => d.Set)
                .Include(d => d.DocumentType)
                .AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                documents = documents.Where(d => d.Title.Contains(searchString));
            }

            if (gradeId.HasValue)
            {
                documents = documents.Where(d => d.GradeSubject.GradeId == gradeId);
                ViewBag.CurrentGradeId = gradeId;
            }
            else
            {
                ViewBag.CurrentGradeId = null;
            }

            if (setId.HasValue)
            {
                documents = documents.Where(d => d.SetId == setId);
                ViewBag.CurrentSetId = setId;
            }
            else
            {
                ViewBag.CurrentSetId = null;
            }

            ViewBag.Grades = _context.Grades.ToList();
            ViewBag.Sets = _context.Sets.ToList();

            return View(documents.ToList());
        }

        [HttpGet]
        [Route("chi-tiet/{id}")]
        public async Task<IActionResult> Detail(int id, int offset = 0, int limit = 5)
        {
            var document = await _context.Documents
                .Include(d => d.User)
                .Include(d => d.DocumentType)
                .Include(d => d.GradeSubject)
                    .ThenInclude(gs => gs.Grade)
                .Include(d => d.Set)
                .Include(d => d.DocumentKeywords)
                    .ThenInclude(dk => dk.Keyword)
                .Include(d => d.UserComments)
                .FirstOrDefaultAsync(d => d.Id == id);

            if (document == null)
            {
                return NotFound();
            }

            var comments = await _context.UserComments
                .Where(c => c.DocumentId == id)
                .OrderByDescending(c => c.CommentDate)
                .Skip(offset)
                .Take(limit)
                .Select(c => new DocumentDetailViewModel.UserComment
                {
                    Id = c.Id,
                    Username = c.User != null ? c.User.UserName : "Anonymous",
                    Rating = c.Rating,
                    Comment = c.Comment,
                    CommentDate = c.CommentDate
                })
                .ToListAsync();

            var relatedBooks = await _context.Documents
                .Where(d => d.Id != document.Id)
                .OrderByDescending(d => d.GradeSubjectId == document.GradeSubjectId && d.DocumentTypeId == document.DocumentTypeId)
                .ThenByDescending(d => d.GradeSubjectId == document.GradeSubjectId || d.DocumentTypeId == document.DocumentTypeId)
                .ThenByDescending(d => d.Downloads)
                .Take(10)
                .ToListAsync();

            var totalComments = await _context.UserComments.CountAsync(c => c.DocumentId == id);
            var averageRating = totalComments > 0 ? (double)_context.UserComments.Where(c => c.DocumentId == id).Average(c => c.Rating) : 0;

            var viewModel = new DocumentDetailViewModel
            {
                Id = document.Id,
                Title = document.Title,
                Slug = document.Slug,
                Description = document.Description,
                GoogleDriveId = document.GoogleDriveId,
                DocumentTypeId = document.DocumentTypeId,
                DocumentType = document.DocumentType,
                GradeSubject = document.GradeSubject,
                Downloads = document.Downloads,
                Views = document.Views,
                User = document.User,
                UploadDate = document.UploadDate,
                UserComments = comments,
                RelatedBooks = relatedBooks,
                DocumentKeywords = document.DocumentKeywords.ToList(),
                TotalComments = totalComments,
                AverageRating = averageRating,
            };

            return View(viewModel);
        }

        [HttpGet]
        [Route("LoadComments")]
        public async Task<IActionResult> LoadCommentsAsync(int documentId, int offset = 0, int limit = 5)
        {
            var comments = await _context.UserComments
                .Where(c => c.DocumentId == documentId)
                .OrderByDescending(c => c.CommentDate)
                .Skip(offset)
                .Take(limit)
                .Select(c => new DocumentDetailViewModel.UserComment
                {
                    Id = c.Id,
                    Username = c.User != null ? c.User.UserName : "Anonymous",
                    Rating = c.Rating,
                    Comment = c.Comment,
                    CommentDate = c.CommentDate
                })
                .ToListAsync();

            if (!comments.Any())
            {
                return Content("");
            }

            return PartialView("_CommentList", comments);
        }

        [HttpPost]
        [Route("SubmitComment")]
        [Authorize]
        public async Task<IActionResult> SubmitComment(string Comment, int Rating, int DocumentId)
        {
            if (string.IsNullOrEmpty(Comment) || Rating < 1 || Rating > 5)
            {
                return BadRequest("Bình luận và đánh giá phải hợp lệ.");
            }

            var document = await _context.Documents.FirstOrDefaultAsync(d => d.Id == DocumentId);
            if (document == null)
            {
                return NotFound("Tài liệu không tồn tại.");
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Unauthorized("Bạn cần đăng nhập để thực hiện đánh giá.");
            }

            var userComment = new UserComment
            {
                UserId = user.Id,
                Comment = Comment,
                Rating = Rating,
                CommentDate = DateTime.Now,
                DocumentId = DocumentId
            };

            _context.UserComments.Add(userComment);
            await _context.SaveChangesAsync();

            return RedirectToAction("Detail", new { id = DocumentId });
        }

        [HttpPost]
        public IActionResult DeleteComment(int id)
        {
            try
            {
                var comment = _context.UserComments.Find(id);
                if (comment != null)
                {
                    _context.UserComments.Remove(comment);
                    _context.SaveChanges();

                    return RedirectToAction("Detail", new { id = comment.DocumentId });
                }
                return NotFound("Không tìm thấy bình luận để xóa.");
            }
            catch (Exception ex)
            {
                return BadRequest("Có lỗi xảy ra: " + ex.Message);
            }
        }

        [HttpGet]
        [Route("tai-lieu-cua-toi")]
        public async Task<IActionResult> UserDocument()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var documents = await _context.Documents
                .Where(d => d.UserId == userId)
                .Include(d => d.GradeSubject)
                .Include(d => d.DocumentType)
                .ToListAsync();

            return View(documents);
        }

        private async Task PopulateSelectLists(int? gradeId = null)
        {
            var grades = await _context.Grades.ToListAsync();
            var documentTypes = await _context.DocumentTypes.ToListAsync();
            var gradeSubjects = gradeId.HasValue
                ? await _context.GradeSubjects.Where(gs => gs.GradeId == gradeId.Value).ToListAsync()
                : new List<GradeSubject>();
            var sets = await _context.Sets.ToListAsync();

            ViewBag.Grades = new SelectList(grades, "Id", "Name");
            ViewBag.GradeSubjects = new SelectList(gradeSubjects, "Id", "Name");
            ViewBag.DocumentTypes = new SelectList(documentTypes, "Id", "Name");
            ViewBag.GradeSubjectsJson = await _context.GradeSubjects.ToListAsync();
            ViewBag.Sets = new SelectList(sets, "Id", "Name");
        }


        [HttpGet]
        [Route("tao-tai-lieu")]
        public async Task<IActionResult> Create()
        {
            await PopulateSelectLists();
            return View();
        }

        [HttpPost]
        [Route("tao-tai-lieu")]
        public async Task<IActionResult> Create(CreateDocumentViewModel model)
        {
            if (ModelState.IsValid)
            {
                var slug = string.Empty;
                var fileId = string.Empty;

                if (model.FileUpload != null && model.FileUpload.Length > 0)
                {
                    slug = _slugService.GenerateSlug(model.Title);
                    var fileExtension = Path.GetExtension(model.FileUpload.FileName);

                    var tempFileName = $"{slug}{fileExtension}";
                    var tempPath = Path.Combine(Path.GetTempPath(), tempFileName);
                    using (var stream = new FileStream(tempPath, FileMode.Create))
                    {
                        await model.FileUpload.CopyToAsync(stream);
                    }

                    fileId = await _googleDriveService.UploadFileAsync(tempPath);
                }

                var document = new Document
                {
                    Title = model.Title,
                    Slug = slug,
                    Description = model.Description,
                    Views = 0,
                    Downloads = 0,
                    GoogleDriveId = fileId,
                    UploadDate = DateTime.Now,
                    IsApproved = User.IsInRole("Admin"),
                    GradeSubjectId = model.GradeSubjectId,
                    DocumentTypeId = model.DocumentTypeId,
                    SetId = model.SetId,
                    UserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                };

                _context.Documents.Add(document);
                await _context.SaveChangesAsync();

                if (!string.IsNullOrEmpty(model.Keywords))
                {
                    var keywords = JsonSerializer.Deserialize<List<Dictionary<string, string>>>(model.Keywords);

                    foreach (var keywordDict in keywords)
                    {
                        var keywordValue = keywordDict.ContainsKey("value") ? keywordDict["value"] : string.Empty;

                        if (string.IsNullOrEmpty(keywordValue))
                        {
                            continue;
                        }

                        var existingKeyword = await _context.Keyword
                            .FirstOrDefaultAsync(k => k.Value == keywordValue);

                        Keyword keywordEntity;

                        if (existingKeyword != null)
                        {
                            keywordEntity = existingKeyword;
                            keywordEntity.UsageCount += 1;
                        }
                        else
                        {
                            keywordEntity = new Keyword
                            {
                                Value = keywordValue,
                                UnsignValue = _slugService.GenerateSlug(keywordValue),
                                UsageCount = 1,
                                CreatedDate = DateTime.Now,
                            };

                            _context.Keyword.Add(keywordEntity);
                            await _context.SaveChangesAsync();
                        }

                        var documentKeyword = new DocumentKeyword
                        {
                            DocumentId = document.Id,
                            KeywordId = keywordEntity.Id
                        };

                        _context.DocumentKeywords.Add(documentKeyword);
                    }
                }

                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Tài liệu đã được tạo thành công!";
                return RedirectToAction("Create");
            }

            await PopulateSelectLists();

            return View(model);
        }

        [HttpGet]
        [Route("cap-nhat-tai-lieu")]
        public async Task<IActionResult> Update(int id)
        {
            var document = await _context.Documents
                .Include(d => d.GradeSubject)
                    .ThenInclude(gs => gs.Grade)
                .Include(d => d.DocumentType)
                .Include(d => d.Set)
                .Include(d => d.DocumentKeywords)
                    .ThenInclude(dk => dk.Keyword)
                .FirstOrDefaultAsync(d => d.Id == id);

            if (document == null)
            {
                return NotFound(); // TODO: Implement custom error response handling later
            }

            var keywords = document.DocumentKeywords
               .Select(dk => new { value = dk.Keyword.Value })
               .ToList();
            ViewBag.KeywordsJson = JsonSerializer.Serialize(keywords);

            var viewModel = new UpdateDocumentViewModel
            {
                Title = document.Title,
                Description = document.Description,
                GoogleDriveId = document.GoogleDriveId,
                GradeId = document.GradeSubject.GradeId,
                Grade = document.GradeSubject.Grade,
                GradeSubjectId = document.GradeSubjectId,
                GradeSubject = document.GradeSubject,
                DocumentTypeId = document.DocumentTypeId,
                SetId = document.SetId,
                Set = document.Set,
            };

            await PopulateSelectLists(document.GradeSubject.GradeId);
            return View(viewModel);
        }

        [HttpPost]
        [Route("cap-nhat-tai-lieu")]
        public async Task<IActionResult> Update(int id, UpdateDocumentViewModel model)
        {
            if (!ModelState.IsValid)
            {
                await PopulateSelectLists(model.GradeId);
                return View(model);
            }

            var document = await _context.Documents
                .Include(d => d.GradeSubject)
                .FirstOrDefaultAsync(d => d.Id == id);

            if (document == null)
            {
                return NotFound(); // TODO: Implement custom error response handling later
            }

            var oldFileId = document.GoogleDriveId;

            document.Title = model.Title;
            document.Description = model.Description;
            document.GradeSubjectId = model.GradeSubjectId;
            document.DocumentTypeId = model.DocumentTypeId;
            document.SetId = model.SetId;

            if (model.FileUpload != null && model.FileUpload.Length > 0)
            {
                var slug = _slugService.GenerateSlug(model.Title);
                var fileExtension = Path.GetExtension(model.FileUpload.FileName);
                var tempFileName = $"{slug}{fileExtension}";
                var tempPath = Path.Combine(Path.GetTempPath(), tempFileName);

                using (var stream = new FileStream(tempPath, FileMode.Create))
                {
                    await model.FileUpload.CopyToAsync(stream);
                }

                var newFileId = await _googleDriveService.UploadFileAsync(tempPath);

                document.GoogleDriveId = newFileId;

                if (!string.IsNullOrEmpty(oldFileId))
                {
                    await _googleDriveService.DeleteFileAsync(oldFileId);
                }
            }

            if (!string.IsNullOrEmpty(model.Keywords))
            {
                var keywords = JsonSerializer.Deserialize<List<Dictionary<string, string>>>(model.Keywords);

                var newKeywordValues = keywords
                    .Where(k => k.ContainsKey("value") && !string.IsNullOrEmpty(k["value"]))
                    .Select(k => k["value"])
                    .ToList();

                var currentKeywords = await _context.DocumentKeywords
                    .Where(dk => dk.DocumentId == document.Id)
                    .Include(dk => dk.Keyword)
                    .ToListAsync();

                var keywordsToRemove = currentKeywords
                    .Where(dk => !newKeywordValues.Contains(dk.Keyword.Value))
                    .ToList();

                var keywordsToAdd = newKeywordValues
                    .Where(k => !currentKeywords.Any(dk => dk.Keyword.Value == k))
                    .ToList();

                foreach (var dk in keywordsToRemove)
                {
                    dk.Keyword.UsageCount = Math.Max(0, dk.Keyword.UsageCount - 1);
                    _context.DocumentKeywords.Remove(dk);
                }

                foreach (var keywordValue in keywordsToAdd)
                {
                    var existingKeyword = await _context.Keyword
                        .FirstOrDefaultAsync(k => k.Value == keywordValue);

                    Keyword keywordEntity;

                    if (existingKeyword != null)
                    {
                        keywordEntity = existingKeyword;
                        keywordEntity.UsageCount += 1;
                    }
                    else
                    {
                        keywordEntity = new Keyword
                        {
                            Value = keywordValue,
                            UnsignValue = _slugService.GenerateSlug(keywordValue),
                            UsageCount = 1,
                            CreatedDate = DateTime.Now,
                        };

                        _context.Keyword.Add(keywordEntity);
                        await _context.SaveChangesAsync();
                    }

                    var documentKeyword = new DocumentKeyword
                    {
                        DocumentId = document.Id,
                        KeywordId = keywordEntity.Id
                    };

                    _context.DocumentKeywords.Add(documentKeyword);
                }
            }

            _context.Documents.Update(document);
            await _context.SaveChangesAsync();

            return RedirectToAction("Update", new { id = id });
        }

        [HttpDelete]
        public async Task<IActionResult> Delete(int id)
        {
            var document = await _context.Documents.FindAsync(id);

            if (document == null)
            {
                return NotFound(); // TODO: Implement custom error response handling later
            }

            var fileId = document.GoogleDriveId;

            _context.Documents.Remove(document);
            await _context.SaveChangesAsync();

            if (!string.IsNullOrEmpty(fileId))
            {
                await _googleDriveService.DeleteFileAsync(fileId);
            }

            return RedirectToAction("Index");
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteMultiple([FromBody] List<int> ids)
        {
            if (ids == null || !ids.Any())
            {
                return BadRequest("Không có tài liệu nào được chọn để xóa.");
            }

            var documentsToDelete = await _context.Documents
                .Where(d => ids.Contains(d.Id))
                .ToListAsync();

            if (documentsToDelete == null || !documentsToDelete.Any())
            {
                return NotFound("Không tìm thấy tài liệu nào để xóa.");
            }

            foreach (var document in documentsToDelete)
            {
                if (!string.IsNullOrEmpty(document.GoogleDriveId))
                {
                    try
                    {
                        await _googleDriveService.DeleteFileAsync(document.GoogleDriveId);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Lỗi khi xóa file trên Google Drive với ID {document.GoogleDriveId}: {ex.Message}");
                    }
                }
            }

            _context.Documents.RemoveRange(documentsToDelete);
            await _context.SaveChangesAsync();

            return Ok();
        }

        [HttpGet]
        [Route("/tim-kiem/{page:int?}")]
        public async Task<IActionResult> Search(string query, int page = 1, int pageSize = 10)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                ViewBag.Message = "Vui lòng nhập từ khóa tìm kiếm.";
                return View(Enumerable.Empty<Document>().ToPagedList(page, pageSize)); // Trả về danh sách trống được phân trang
            }

            // Tách query thành các từ khóa nhỏ
            var keywords = query.ToLower().Split(' ', StringSplitOptions.RemoveEmptyEntries);

            // Tìm kiếm ưu tiên trong tiêu đề
            var titleResults = await _context.Documents
                .Include(d => d.DocumentKeywords)
                    .ThenInclude(dk => dk.Keyword)
                .Include(d => d.User)
                .Where(d => d.Title.ToLower().Contains(query.ToLower())) // Tìm khớp nguyên cụm trong tiêu đề
                .ToListAsync();

            // Tìm kiếm mở rộng trong từ khóa
            var keywordResults = await _context.Documents
                .Include(d => d.DocumentKeywords)
                    .ThenInclude(dk => dk.Keyword)
                .Include(d => d.User)
                .Where(d =>
                    d.DocumentKeywords.Any(dk =>
                        keywords.Any(k => dk.Keyword.Value.ToLower().Contains(k)))) // Tìm từng từ khóa trong danh sách
                .ToListAsync();

            // Tạo danh sách kết quả tạm thời để tính toán số lượng trùng khớp
            var allResults = titleResults.Concat(keywordResults).Distinct().ToList();

            // Tính toán số lượng từ khóa trùng trong tiêu đề và từ khóa của tài liệu
            var resultsWithMatchCount = allResults.Select(doc => new
            {
                Document = doc,
                TitleMatchCount = doc.Title.ToLower().Split(' ').Count(word => keywords.Contains(word)), // Số từ khóa trùng trong tiêu đề
                KeywordMatchCount = doc.DocumentKeywords.Count(dk => keywords.Contains(dk.Keyword.Value.ToLower())) // Số từ khóa trùng trong từ khóa
            }).ToList();

            // Sắp xếp tài liệu theo tổng số từ khóa trùng (trong tiêu đề + từ khóa)
            var sortedResults = resultsWithMatchCount
                .OrderByDescending(x => x.TitleMatchCount + x.KeywordMatchCount) // Sắp xếp theo tổng số từ khóa trùng
                .Select(x => x.Document) // Chọn lại tài liệu sau khi đã tính toán và sắp xếp
                .ToList();

            // Sử dụng X.PagedList để phân trang
            var pagedResults = sortedResults.ToPagedList(page, pageSize);

            ViewBag.Query = query;
            if (!pagedResults.Any())
            {
                ViewBag.Message = "Không tìm thấy tài liệu nào.";
            }

            return View(pagedResults); // Trả về danh sách phân trang
        }
    }
}