using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StudyResource.Data;
using StudyResource.ViewModels.Home;

namespace StudyResource.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public HomeController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var today = DateTime.Now;
            var sevenDaysAgo = today.AddDays(-7);

            var uploadCount = new List<int>();
            for (int i = 6; i >= 0; i--)
            {
                var dayStart = today.AddDays(-i).Date;
                var dayEnd = dayStart.AddDays(1).AddSeconds(-1);

                var count = await _context.Documents
                    .Where(d => d.UploadDate >= dayStart && d.UploadDate <= dayEnd)
                    .CountAsync();
                uploadCount.Add(count);
            }

            var downloadCount = new List<int>();
            for (int i = 6; i >= 0; i--)
            {
                var dayStart = today.AddDays(-i).Date;
                var dayEnd = dayStart.AddDays(1).AddSeconds(-1);

                var count = await _context.DownloadHistories
                    .Where(d => d.DownloadDate >= dayStart && d.DownloadDate <= dayEnd)
                    .CountAsync();
                downloadCount.Add(count);
            }

            var pendingDocuments = await _context.Documents
                .Where(d => !d.IsApproved)
                .CountAsync();

            var model = new AdminDashboardViewModel
            {
                PendingDocuments = pendingDocuments,
                DownloadCount = downloadCount,
                UploadCount = uploadCount
            };

            return View(model);
        }
    }
}
