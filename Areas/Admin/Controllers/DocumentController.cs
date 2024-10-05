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

        private async Task PopulateSelectLists()
        {
            var grades = await _context.Grades.ToListAsync();
            var subjects = await _context.Subjects.ToListAsync();
            var gradeSubjects = await _context.GradeSubjects.ToListAsync();
            var documentTypes = await _context.DocumentTypes.ToListAsync();

            ViewBag.Grades = new SelectList(grades, "Id", "Name");
            ViewBag.Subjects = new SelectList(subjects, "Id", "Name");
            ViewBag.GradeSubjects = gradeSubjects;
            ViewBag.DocumentTypes = new SelectList(documentTypes, "Id", "Name");
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
                    GradeSubjectId = model.GradeSubjectId,
                    DocumentTypeId = model.DocumentTypeId
                };

                _context.Documents.Add(document);
                await _context.SaveChangesAsync();

                return RedirectToAction("Create");
            }

            await PopulateSelectLists();

            return View(model);
        }
    }
}
