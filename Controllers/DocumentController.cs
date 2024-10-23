using Microsoft.AspNetCore.Mvc;
using StudyResource.Data;
using StudyResource.Models;
using Microsoft.EntityFrameworkCore;

namespace StudyResource.Controllers
{
    public class DocumentController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DocumentController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index(int? grade, int documentType = 1, int page = 1)
        {
            int pageSize = 6;

            // Filter documents based on the grade and document type
            var query = _context.Documents
                .Include(d => d.GradeSubject)
                .Include(d => d.DocumentType)
                .AsQueryable();

            if (grade.HasValue)
            {
                query = query.Where(d => d.GradeSubjectId == grade.Value);
            }

            query = query.Where(d => d.DocumentTypeId == documentType);

            // Pagination
            var totalDocuments = query.Count();
            var documents = query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            // Prepare the view model
            var viewModel = new DocumentViewModel
            {
                Documents = documents,
                Grades = _context.GradeSubjects.ToList(),
                SelectedGrade = grade,
                SelectedDocumentType = documentType,
                CurrentPage = page,
                TotalPages = (int)Math.Ceiling(totalDocuments / (double)pageSize)
            };

            return View(viewModel);
        }

        // Filter documents by grade
        public IActionResult FilterByGrade(int? grade, int documentType = 1)
        {
            return RedirectToAction("Index", new { grade = grade, documentType = documentType });
        }

        // Search documents by query
        public IActionResult Search(string query, int page = 1)
        {
            int pageSize = 6;

            // Search documents based on the query
            var queryable = _context.Documents
                .Include(d => d.GradeSubject)
                .Include(d => d.DocumentType)
                .AsQueryable();

            if (!string.IsNullOrEmpty(query))
            {
                queryable = queryable.Where(d => d.Title.Contains(query) ||
                         d.Description.Contains(query) ||
                         d.GradeSubject.Name.Contains(query) ||
                         d.DocumentType.Name.Contains(query));
            }

            // Pagination
            var totalDocuments = queryable.Count();
            var documents = queryable
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            // Prepare the view model
            var viewModel = new DocumentViewModel
            {
                Documents = documents,
                Grades = _context.GradeSubjects.ToList(),
                Query = query,
                CurrentPage = page,
                TotalPages = (int)Math.Ceiling(totalDocuments / (double)pageSize)
            };

            return View("Index", viewModel);
        }
    }
}