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

        [HttpPost]
        public async Task<ActionResult> SaveHistory(string? userId, int documentId)
        {
            if (documentId <= 0)
            {
                return BadRequest("Invalid data.");
            }

            var downloadHistory = new DownloadHistory
            {
                UserId = userId,
                DocumentId = documentId,
                DownloadDate = DateTime.Now
            };

            _context.DownloadHistories.Add(downloadHistory);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Lưu lịch sử tải xuống thành công." });    
        }

    }
}
