using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StudyResource.Data;
using StudyResource.Models;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

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
        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var downloadHistories = await _context.DownloadHistories
                .Include(dh => dh.Document)
                    .ThenInclude(d => d.GradeSubject)
                .Where(dh => dh.UserId == userId )
                .OrderByDescending(dh => dh.DownloadDate)
                .ToListAsync();

            return View(downloadHistories);
        }

        [HttpPost]
        public async Task<ActionResult> SaveHistory(string? userName, int documentId)
        {
            if (documentId <= 0)
            {
                return BadRequest("Invalid data.");
            }

            string? userId = null;

            if (!string.IsNullOrEmpty(userName))
            {
                var user = await _context.Users.SingleOrDefaultAsync(u => u.UserName == userName);
                if (user == null)
                {
                    return NotFound("User not found.");
                }
                userId = user.Id;
            }

            var document = await _context.Documents.SingleOrDefaultAsync(d => d.Id == documentId);
            if (document == null)
            {
                return NotFound("Document not found.");
            }

            document.Downloads++;
            _context.Documents.Update(document);

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
