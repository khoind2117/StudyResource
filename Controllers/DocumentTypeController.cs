using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using X.PagedList.Extensions;
using StudyResource.Data;
using StudyResource.Models;
using StudyResource.Services;
using StudyResource.ViewModels.DocumentType;
using System.Net.WebSockets;

namespace StudyResource.Controllers
{
    [Route("loai-tai-lieu")]
    public class DocumentTypeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DocumentTypeController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        [Route("tai-lieu-tham-khao/{gradeId?}/{subjectId?}")]
        public async Task<IActionResult> Index(int? gradeId = 1, int? subjectId = 1, int page = 1, int pageSize = 10)
        {
            var documentsQuery = _context.Documents
                .Include(d => d.User)
                .Include(d => d.GradeSubject)
                    .ThenInclude(gs => gs.Grade)
                .Include(d => d.GradeSubject)
                    .ThenInclude(gs => gs.Subject)
                .AsSplitQuery()
                .AsQueryable();

            
            if (gradeId.HasValue)
            {
                documentsQuery = documentsQuery.Where(d => d.GradeSubject.GradeId == gradeId);
            }

            if (subjectId.HasValue)
            {
                documentsQuery = documentsQuery.Where(d => d.GradeSubject.SubjectId == subjectId);
            }

            var documentTypeId = await _context.DocumentTypes
                .Where(d => d.Name.ToLower() == "tài liệu tham khảo")
                .Select(d => d.Id)
                .FirstOrDefaultAsync();

            documentsQuery = documentsQuery.Where(d => d.DocumentTypeId == documentTypeId);

            var totalDocuments = await documentsQuery.CountAsync();
            var documents = documentsQuery.OrderByDescending(d => d.UploadDate).ToPagedList(page, pageSize);
            if (!documents.Any())
            {
                ViewBag.Message = $"Không tìm thấy tài liệu nào.";
            }

            ViewBag.GradeName = await _context.Grades
                .Where(g => g.Id == gradeId)
                .Select(g => g.Name)
                .AsNoTracking()
                .FirstOrDefaultAsync();

            ViewBag.SubjectName = await _context.Subjects
                .Where(s => s.Id == subjectId)
                .Select(s => s.Name)
                .AsNoTracking()
                .FirstOrDefaultAsync();
            ViewBag.Page = page;

            return View(documents);
        }

        [HttpGet]
        [Route("sach-giao-khoa/{setSlug?}/{gradeSlug?}")]
        public async Task<IActionResult> Textbook(string gradeSlug = "lop-1", string setSlug = "canh-dieu")
        {
            var grade = await _context.Grades.FirstOrDefaultAsync(g => g.Slug == gradeSlug);
            var set = await _context.Sets.FirstOrDefaultAsync(s => s.Slug == setSlug);
            int? GradeId = grade?.Id;
            int? SetId = set?.Id;

            var documents = await _context.Documents
                .Include(d => d.GradeSubject)
                .Include(d => d.DocumentType)
                .Where(d => d.DocumentType.Id == 1)
                .Where(d => !GradeId.HasValue || d.GradeSubject.GradeId == GradeId)
                .Where(d => !SetId.HasValue || d.SetId == SetId)
                .AsSplitQuery()
                .AsNoTracking()
                .ToListAsync();

            ViewBag.CurrentGradeSlug = gradeSlug;
            ViewBag.CurrentSetSlug = setSlug;

            return View(documents);
        }

        [HttpGet]
        [Route("sach-bai-tap/{setSlug?}/{gradeSlug?}")]
        public async Task<IActionResult> WorkBook(string gradeSlug = "lop-1", string setSlug = "canh-dieu")
        {
            var grade = await _context.Grades.FirstOrDefaultAsync(g => g.Slug == gradeSlug);
            var set = await _context.Sets.FirstOrDefaultAsync(s => s.Slug == setSlug);
            int? GradeId = grade?.Id;
            int? SetId = set?.Id;

            var documents = await _context.Documents
                .Include(d => d.GradeSubject)
                .Include(d => d.DocumentType)
                .Where(d => d.DocumentType.Id == 2)
                .Where(d => !GradeId.HasValue || d.GradeSubject.GradeId == GradeId)
                .Where(d => !SetId.HasValue || d.SetId == SetId)
                .AsSplitQuery()
                .AsNoTracking()
                .ToListAsync();

            ViewBag.CurrentGradeSlug = gradeSlug;
            ViewBag.CurrentSetSlug = setSlug;

            return View(documents);
        }

        [HttpGet]
        [Route("sach-giao-vien/{setSlug?}/{gradeSlug?}")]
        public async Task<IActionResult> TeacherBook(string gradeSlug = "lop-1", string setSlug = "canh-dieu")
        {
            var grade = await _context.Grades.FirstOrDefaultAsync(g => g.Slug == gradeSlug);
            var set = await _context.Sets.FirstOrDefaultAsync(s => s.Slug == setSlug);
            int? GradeId = grade?.Id;
            int? SetId = set?.Id;

            var documents = await _context.Documents
                .Include(d => d.GradeSubject)
                .Include(d => d.DocumentType)
                .Where(d => d.DocumentType.Id == 2)
                .Where(d => !GradeId.HasValue || d.GradeSubject.GradeId == GradeId)
                .Where(d => !SetId.HasValue || d.SetId == SetId)
                .AsSplitQuery()
                .AsNoTracking()
                .ToListAsync();

            ViewBag.CurrentGradeSlug = gradeSlug;
            ViewBag.CurrentSetSlug = setSlug;

            return View(documents);
        }

        [HttpGet]
        [Route("tai-lieu-tham-khao")]
        public async Task<IActionResult> ReferenceBook()
        {
            var slickDocuments = await _context.Documents
                .OrderByDescending(d => d.UploadDate)
                .Take(5)
                .AsNoTracking()
                .ToListAsync();

            var documentTypeId = await _context.DocumentTypes
                .Where(d => d.Name.ToLower() == "tài liệu tham khảo")
                .Select(d => d.Id)
                .FirstOrDefaultAsync();

            ViewBag.DocumentTypeId = documentTypeId;

            var highlyRatedDocuments = await _context.Documents
                .Include(d => d.UserComments)
                .Where(d => d.UserComments.Any() && d.DocumentTypeId == documentTypeId)
                .Select(d => new
                {
                    Document = d,
                    AverageRating = d.UserComments.Average(uc => uc.Rating),
                })
                .OrderByDescending(x => x.AverageRating)
                .Take(5)
                .Select(x => x.Document)
                .AsSplitQuery()
                .AsNoTracking()
                .ToListAsync();
            
            var grades = await _context.Grades
                .Include(g => g.GradeSubjects)
                    .ThenInclude(gs => gs.Documents)
                .Include(g => g.GradeSubjects)
                    .ThenInclude(gs => gs.Subject)
                .AsSplitQuery()
                .AsNoTracking()
                .ToListAsync();

            var model = new ReferenceBookViewModel
            {
                SlickDocuments = slickDocuments,
                HighlyRatedDocuments = highlyRatedDocuments,
                Grades = grades
            };

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> SidebarReferenceBook(string filterType)
        {
            return ViewComponent("SidebarReferenceBook", new { filterType });
        }
    }
}
