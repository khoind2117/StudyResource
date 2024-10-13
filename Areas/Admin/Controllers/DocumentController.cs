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

        private async Task PopulateSelectLists(int? gradeId = null)
        {
            var grades = await _context.Grades.ToListAsync();
            var documentTypes = await _context.DocumentTypes.ToListAsync();
            var gradeSubjects = gradeId.HasValue
                ? await _context.GradeSubjects.Where(gs => gs.GradeId == gradeId.Value).ToListAsync()
                : new List<GradeSubject>();

            ViewBag.Grades = new SelectList(grades, "Id", "Name");
            ViewBag.GradeSubjects = new SelectList(gradeSubjects, "Id", "Name");
            ViewBag.DocumentTypes = new SelectList(documentTypes, "Id", "Name");
            ViewBag.GradeSubjectsJson = await _context.GradeSubjects.ToListAsync();
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

        [HttpGet]
        public async Task<IActionResult> Update(int id)
        {
            var document = await _context.Documents
                .Include(d => d.GradeSubject)
                    .ThenInclude(gs => gs.Grade)
                .Include(d => d.DocumentType)
                .FirstOrDefaultAsync(d => d.Id == id);

            if (document == null)
            {
                return NotFound(); // TODO: Implement custom error response handling later
            }

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
            };

            await PopulateSelectLists(document.GradeSubject.GradeId);
            return View(viewModel);
        }

        [HttpPut]
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

                _context.Documents.Update(document);
                await _context.SaveChangesAsync();
            }

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

        [HttpGet]
        public async Task<IActionResult> Ebook()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> PDFViewer()
        {
            return View();
        }
    }
}
