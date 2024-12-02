using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StudyResource.Data;
using StudyResource.Models;
using StudyResource.Services;

namespace StudyResource.Controllers
{
    [Route("tu-khoa")]
    public class KeywordController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly SlugService _slugService;

        public KeywordController(ApplicationDbContext context,
            SlugService slugService)
        {
            _context = context;
            _slugService = slugService;
        }

        [Route("{keyword}")]
        public async Task<IActionResult> Index(string keyword)
        {
            if (string.IsNullOrEmpty(keyword))
            {
                return RedirectToAction("Index", "Home");
            }

            var documents = await _context.Documents
               .Where(d => d.DocumentKeywords
                   .Any(dt => dt.Keyword.UnsignValue.Contains(_slugService.GenerateSlug(keyword))))
               .Include(d => d.User)
               .Include(d => d.DocumentKeywords)
                   .ThenInclude(dk => dk.Keyword)
               .ToListAsync();

            if (!documents.Any())
            {
                ViewBag.Message = $"Không tìm thấy tài liệu nào với từ khóa '{keyword}'.";
            }

            ViewBag.Keyword = keyword;
            return View(documents);
        }

    }
}
