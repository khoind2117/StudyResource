using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StudyResource.Data;
using StudyResource.Models;
using StudyResource.Services;
using X.PagedList.Extensions;

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

        [Route("{keyword}/{page:int?}")]
        public async Task<IActionResult> Index(string keyword, int page = 1, int pageSize = 10)
        {
            if (string.IsNullOrEmpty(keyword))
            {
                return RedirectToAction("Index", "Home");
            }

            var documentsQuery = _context.Documents
                .Where(d => d.DocumentKeywords
                    .Any(dt => dt.Keyword.UnsignValue.Contains(_slugService.GenerateSlug(keyword))))
                .Include(d => d.User)
                .Include(d => d.DocumentKeywords)
                    .ThenInclude(dk => dk.Keyword);

            var documents = documentsQuery.ToPagedList(page, pageSize);

            if (!documents.Any())
            {
                ViewBag.Message = $"Không tìm thấy tài liệu nào với từ khóa '{keyword}'.";
            }

            ViewBag.Keyword = keyword;
            ViewBag.Page = page;

            return View(documents);
        }
    }
}
