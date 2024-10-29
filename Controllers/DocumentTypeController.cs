using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StudyResource.Data;

namespace StudyResource.Controllers
{
    public class DocumentTypeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DocumentTypeController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(string? slug)
        {
            var grades = await _context.Grades.ToListAsync();
            ViewBag.Grades = grades;
            var sets = await _context.Sets.ToListAsync();
            ViewBag.Sets = sets;

            var documents = await _context.Documents
                .Include(d => d.DocumentType)
                .Include(d => d.Set)
                .Where(d => d.DocumentType.Slug == slug).ToListAsync();

            return View(documents);
        }
    }
}
