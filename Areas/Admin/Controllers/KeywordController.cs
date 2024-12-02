using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StudyResource.Data;

namespace StudyResource.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class KeywordController : Controller
    {
        private readonly ApplicationDbContext _context;

        public KeywordController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var keywords = await _context.Keyword.ToListAsync();
            return View(keywords);
        }
    }
}
