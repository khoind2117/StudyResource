using CsvHelper;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using StudyResource.Data;
using StudyResource.Models;
using StudyResource.Services;
using StudyResource.ViewModels.Document;
using System;
using System.Globalization;
using System.Linq;
using System.Security.Claims;
using System.Text.Json;
using System.Threading.Tasks;
using static Microsoft.Extensions.Logging.EventSource.LoggingEventSource;

namespace StudyResource.Controllers
{
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
        public async Task<IActionResult> Ebook(int id)
        {
            var document = await _context.Documents.FindAsync(id);
            string fileUrl = Url.Action("DownloadFile", "GoogleDrive", new { fileId = document.GoogleDriveId });
            ViewData["DefaultFileUrl"] = fileUrl;
            ViewData["DocId"] = document.Id;
            return View();
        }

        [HttpGet]
        public IActionResult Index(string searchString, int? gradeId, int? setId)
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
        public async Task<IActionResult> Detail(int id)
        {
            var document = await _context.Documents
                .Include(d => d.DocumentType)
                .Include(d => d.GradeSubject)
                .ThenInclude(gs => gs.Grade)
                .Include(d => d.Set)
                .Include(d => d.DocumentKeywords)
                    .ThenInclude(dk => dk.Keyword)
                .FirstOrDefaultAsync(d => d.Id == id);

            if (document == null)
            {
                return NotFound();
            }

            var comments = await _context.UserComments
                .Where(c => c.DocumentId == id)
                .Select(c => new DocumentDetailViewModel.UserComment
                {
                    Id = c.Id, 
                    Username = c.User != null ? c.User.UserName : "Anonymous",
                    Comment = c.Comment,
                    CommentDate = c.CommentDate
                })
                .ToListAsync();

            var keywords = document.DocumentKeywords.Select(dk => dk.Keyword.Value).ToList();

            ViewBag.Keywords = keywords;

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
                UserComments = comments,
                RelatedBooks = new List<Models.Document>() 
            };

            return View(viewModel);
        }

        [HttpGet]
        public async Task<IActionResult> RelatedBooks(int gradeSubjectId, int documentTypeId)
        {
            var relatedBooks = await _context.Documents
                .Include(d => d.DocumentType)
                .Where(d => d.GradeSubjectId == gradeSubjectId || d.DocumentTypeId == documentTypeId)
                .Take(6)
                .ToListAsync();

            return PartialView("_RelatedBooksPartial", relatedBooks);
        }

        [HttpPost]
        public async Task<IActionResult> SubmitComment(string Comment, string GoogleDriveId)
        {
            if (ModelState.IsValid)
            {
                var document = await _context.Documents.FirstOrDefaultAsync(d => d.GoogleDriveId == GoogleDriveId);
                if (document == null)
                {
                    return NotFound();
                }

                var user = await _userManager.GetUserAsync(User);

                var userComment = new UserComment
                {
                    UserId = user?.Id,
                    CommentDate = DateTime.Now,
                    Comment = Comment,
                    DocumentId = document.Id,
                    Rating = 0
                };

                _context.UserComments.Add(userComment);
                await _context.SaveChangesAsync();

                return RedirectToAction("Detail", new { id = document.Id });
            }

            return BadRequest();
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

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            await PopulateSelectLists();
            return View();
        }

        [HttpPost]
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
    }
}