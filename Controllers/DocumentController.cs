using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StudyResource.Data;
using StudyResource.Models;
using StudyResource.ViewModels.Document;

namespace StudyResource.Controllers
{
    public class DocumentController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DocumentController(ApplicationDbContext context)
        {
            _context = context;
        }

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
        .Include(d => d.Set)
        .FirstOrDefaultAsync(d => d.Id == id);

            if (document == null)
            {
                return NotFound();
            }

            var viewModel = new DocumentDetailViewModel
            {
                Id = document.Id,
                Title = document.Title,
                Description = document.Description,
                GoogleDriveId = document.GoogleDriveId,
                DocumentTypeId = document.DocumentTypeId,
                DocumentType = document.DocumentType,
                UserNotes = string.Empty,
                UserComments = await _context.UserComments
                    .Where(c => c.DocumentId == id)
                    .Select(c => new DocumentDetailViewModel.UserComment
                    {
                        Username = c.Username,
                        Comment = c.Comment,
                        Rating = c.Rating,
                        CommentDate = c.CommentDate
                    })
                    .ToListAsync()
            };

            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> AddComment(UserComment userComment, int documentId)
        {
            if (ModelState.IsValid)
            {
                userComment.CommentDate = DateTime.Now;
                userComment.DocumentId = documentId;

                _context.UserComments.Add(userComment);
                await _context.SaveChangesAsync();

                return RedirectToAction("Detail", new { id = documentId });
            }

            return RedirectToAction("Detail", new { id = documentId });
        }


    }
}