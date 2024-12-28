using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StudyResource.Data;

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

        public async Task<IActionResult> Index()
        {
            return View();
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
                .ToListAsync();

            ViewBag.CurrentGradeSlug = gradeSlug;
            ViewBag.CurrentSetSlug = setSlug;

            return View(documents);
        }

        [HttpGet]
        [Route("tai-lieu-tham-khao")]
        public async Task<IActionResult> ReferenceBook()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> SidebarReferenceBook(string filterType)
        {
            return ViewComponent("SidebarReferenceBook", new { filterType });
        }
    }
}
