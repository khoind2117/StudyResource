using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using StudyResource.Data;
using StudyResource.Models;
using StudyResource.Services;
using StudyResource.ViewModels.Document;

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
            var document = await _context.Documents
                .Include(d => d.GradeSubject)
                .Include(d => d.DocumentType)
                .OrderByDescending(d => d.UploadDate)
                .ToListAsync();

            return View(document);
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var grades = await _context.Grades.ToListAsync();
            var subjects = await _context.Subjects.ToListAsync();
            var gradeSubjects = await _context.GradeSubjects.ToListAsync();
            var documentTypes = await _context.DocumentTypes.ToListAsync();

            ViewBag.Grades = new SelectList(grades, "Id", "Name");
            ViewBag.Subjects = new SelectList(subjects, "Id", "Name");
            ViewBag.GradeSubjects = gradeSubjects;
            ViewBag.DocumentTypes = new SelectList(documentTypes, "Id", "Name");
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateDocumentViewModel model)
        {
            if (ModelState.IsValid)
            {
                var filePath = string.Empty;

                if (model.FileUpload != null && model.FileUpload.Length > 0)
                {
                    model.Slug = _slugService.GenerateSlug(model.Title);
                    var fileExtension = Path.GetExtension(model.FileUpload.FileName);

                    var tempFileName = $"{model.Slug}{fileExtension}";
                    var tempPath = Path.Combine(Path.GetTempPath(), tempFileName);
                    using (var stream = new FileStream(tempPath, FileMode.Create))
                    {
                        await model.FileUpload.CopyToAsync(stream);
                    }

                    var fileId = await _googleDriveService.UploadFileAsync(tempPath);
                    filePath = _googleDriveService.GetFileLink(fileId);
                    model.UploadDate = DateTime.Now;
                }

                var document = new Document
                {
                    Title = model.Title,
                    Slug = model.Slug,
                    Description = model.Description,
                    Views = model.Views,
                    Downloads = model.Downloads,
                    FilePath = filePath,
                    UploadDate = model.UploadDate,
                    GradeSubjectId = model.GradeSubjectId,
                    DocumentTypeId = model.DocumentTypeId
                };

                _context.Documents.Add(document);
                await _context.SaveChangesAsync();

                return RedirectToAction("Create");
            }

            return View(model);
        }
    }
}
