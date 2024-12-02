using CsvHelper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using StudyResource.Data;
using StudyResource.Models;
using StudyResource.Services;
using StudyResource.ViewModels.Document;
using System.Globalization;
using System.Security.Claims;
using System.Text.Json;

namespace StudyResource.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class DocumentController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly GoogleDriveService _googleDriveService;
        private readonly SlugService _slugService;

        public DocumentController(ApplicationDbContext context,
            GoogleDriveService googleDriveService,
            SlugService slugService)
        {
            _context = context;
            _googleDriveService = googleDriveService;
            _slugService = slugService;
        }

        public async Task<IActionResult> IndexAsync()
        {
            var documents = await _context.Documents
              .Include(d => d.GradeSubject)
              .Include(d => d.DocumentType)
              .Include(d => d.User)
              .OrderByDescending(d => d.UploadDate)
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
        public async Task<IActionResult> UploadCsv()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> UploadCsv(UploadCsvViewModel model)
        {
            if (ModelState.IsValid)
            {
                var csvFile = model.CsvFile;
                if (csvFile != null && csvFile.Length > 0)
                {
                    try
                    {
                        using (var reader = new StreamReader(csvFile.OpenReadStream()))
                        using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
                        {
                            // Đăng ký DocumentMap
                            csv.Context.RegisterClassMap<DocumentMap>();

                            var records = csv.GetRecords<DocumentCsvViewModel>().ToList();

                            var errorList = new List<string>();
                            int rowIndex = 2;

                            foreach (var record in records)
                            {

                                if (string.IsNullOrWhiteSpace(record.Title) ||
                                   string.IsNullOrWhiteSpace(record.GradeSubjectName) ||
                                   string.IsNullOrWhiteSpace(record.DocumentTypeName) ||
                                   string.IsNullOrWhiteSpace(record.GoogleDriveId))
                                {
                                    errorList.Add($"Hàng {rowIndex}: Một hoặc nhiều cột bị thiếu dữ liệu.");
                                    rowIndex++;
                                    continue;
                                }

                                var gradeSubject = await _context.GradeSubjects
                                    .FirstOrDefaultAsync(gs => gs.Name.ToLower() == record.GradeSubjectName.ToLower());
                                if (gradeSubject == null)
                                {
                                    errorList.Add($"Hàng {rowIndex}: Môn học '{record.GradeSubjectName}' không tồn tại.");
                                    rowIndex++;
                                    continue;
                                }

                                var documentType = await _context.DocumentTypes
                                    .FirstOrDefaultAsync(dt => dt.Name.ToLower() == record.DocumentTypeName.ToLower());
                                if (documentType == null)
                                {
                                    errorList.Add($"Hàng {rowIndex}: Loại tài liệu '{record.DocumentTypeName}' không tồn tại.");
                                    rowIndex++;
                                    continue;
                                }

                                Set set = null;
                                if (!string.IsNullOrWhiteSpace(record.SetName)) 
                                {
                                    set = await _context.Sets
                                        .FirstOrDefaultAsync(s => s.Name.ToLower() == record.SetName.ToLower());
                                    if (set == null)
                                    {
                                        errorList.Add($"Hàng {rowIndex}: Bộ sách '{record.SetName}' không tồn tại.");
                                        rowIndex++;
                                        continue;
                                    }
                                }

                                var documentKeywords = new List<DocumentKeyword>();
                                if (!string.IsNullOrWhiteSpace(record.Keyword))
                                {
                                    var keywords = record.Keyword.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                                    foreach (var keyword in keywords)
                                    {
                                        var trimmedKeyword = keyword.Trim();
                                        var existingKeyword = await _context.Keyword
                                            .FirstOrDefaultAsync(k => k.Value.ToLower() == trimmedKeyword.ToLower());

                                        if (existingKeyword == null)
                                        {
                                            existingKeyword = new Keyword 
                                            { 
                                                Value = trimmedKeyword,
                                                UnsignValue = _slugService.GenerateSlug(trimmedKeyword),
                                                UsageCount = 1,
                                                CreatedDate = DateTime.Now,
                                            };
                                            _context.Keyword.Add(existingKeyword);
                                            await _context.SaveChangesAsync();
                                        }
                                        else
                                        {
                                            existingKeyword.UsageCount += 1;
                                            _context.Keyword.Update(existingKeyword);
                                            await _context.SaveChangesAsync();
                                        }

                                        documentKeywords.Add(new DocumentKeyword { KeywordId = existingKeyword.Id });
                                    }
                                }

                                var document = new Document
                                {
                                    Title = record.Title,
                                    Slug = _slugService.GenerateSlug(record.Title),
                                    Description = record.Description,
                                    GoogleDriveId = record.GoogleDriveId,
                                    UploadDate = DateTime.Now,
                                    IsApproved = User.IsInRole("Admin"),
                                    GradeSubjectId = gradeSubject.Id,
                                    DocumentTypeId = documentType.Id,
                                    SetId = set?.Id,
                                    UserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value,
                                    DocumentKeywords = documentKeywords
                                };

                                _context.Documents.Add(document);
                                rowIndex++;
                            }

                            await _context.SaveChangesAsync();

                            if (errorList.Any())
                            {
                                TempData["ErrorList"] = errorList;
                            }
                            else
                            {
                                TempData["SuccessMessage"] = "Dữ liệu đã được nhập thành công!";
                            }

                            return RedirectToAction("UploadCsv");
                        }
                    }
                    catch (Exception ex)
                    {
                        ModelState.AddModelError("", "Đã xảy ra lỗi trong quá trình tải file CSV. Vui lòng kiểm tra file và thử lại.");
                    }
                }
            }

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

            return Ok();
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteSelectedDocuments([FromBody] List<int> ids)
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
        public async Task<IActionResult> PendingDocuments()
        {
            var pendingDocuments = await _context.Documents
                .Where(d => !d.IsApproved)
                .Include(d => d.User)
                .Include(d => d.GradeSubject)
                .Include(d => d.DocumentType)
                .Include(d => d.Set)
                .ToListAsync();

            return View(pendingDocuments);
        }

        [HttpPost]
        public async Task<IActionResult> PendingDocuments(int id)
        {
            var document = await _context.Documents.FindAsync(id);
            if (document == null)
            {
                return NotFound();
            }

            document.IsApproved = true;
            _context.Documents.Update(document);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Tài liệu đã được duyệt thành công.";
            return RedirectToAction("PendingDocuments");
        }

        [HttpPost]
        public async Task<IActionResult> ApproveDocument(int id)
        {
            var document = await _context.Documents.FindAsync(id);
            if (document == null)
            {
                return NotFound();
            }

            document.IsApproved = true;

            await _context.SaveChangesAsync();
            return Ok();
        }

        [HttpPost]
        public async Task<IActionResult> ApproveSelectedDocuments([FromBody] List<int> documentIds)
        {
            if (documentIds == null || !documentIds.Any())
            {
                return BadRequest("Không có tài liệu nào được chọn để duyệt.");
            }

            var documentsToApprove = await _context.Documents
                .Where(d => documentIds.Contains(d.Id) && !d.IsApproved)
                .ToListAsync();

            if (documentsToApprove == null || !documentsToApprove.Any())
            {
                return NotFound("Không tìm thấy tài liệu nào cần duyệt.");
            }

            foreach (var document in documentsToApprove)
            {
                document.IsApproved = true;
            }

            await _context.SaveChangesAsync();

            return Ok();
        }

        [HttpGet]
        public async Task<IActionResult> ApprovedDocuments()
        {
            var pendingDocuments = await _context.Documents
                .Where(d => d.IsApproved)
                .Include(d => d.User)
                .Include(d => d.GradeSubject)
                .Include(d => d.DocumentType)
                .Include(d => d.Set)
                .ToListAsync();

            return View(pendingDocuments);
        }
    }
}
