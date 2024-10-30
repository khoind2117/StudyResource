using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StudyResource.Data;
using StudyResource.Models;

namespace StudyResource.Controllers
{
    public class DownloadHistoryController : Controller
    {
        private readonly ApplicationDbContext _context;
        public DownloadHistoryController(ApplicationDbContext context)
        {
            _context = context;    
        }

        [HttpGet]
        public async Task<IActionResult> Index(string userId)
        {
            var downloadHistories = await _context.DownloadHistories
                .Include(dh => dh.Document)
                .Where(dh => dh.UserId == userId )
                .OrderByDescending(dh => dh.DownloadDate)
                .ToListAsync();

            return View(downloadHistories);
        }

    }
}
