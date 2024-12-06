using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StudyResource.Data;
using StudyResource.ViewModels.Document;
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
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> GetUploadAndDownloadDocuments()
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

            var model = new UploadAndDownloadDocumentViewModel
            {
                DownloadCount = downloadCount,
                UploadCount = uploadCount
            };

            return PartialView("_UploadAndDownloadDocuments", model);
        }

        [HttpGet]
        public async Task<IActionResult> GetPendingAndApprovedDocuments()
        {
            var today = DateTime.Today;

            var approvedCountToday = await _context.Documents
                .CountAsync(d => d.IsApproved && d.UploadDate.Date == today);
            var pendingCount = await _context.Documents
                .CountAsync(d => !d.IsApproved);
            var approvedCount = await _context.Documents
                .CountAsync(d => d.IsApproved);

            var totalDocuments = approvedCount + pendingCount;
            int approvalPercentage = 0;
            if (totalDocuments > 0)
            {
                approvalPercentage = (int)((double)approvedCount / totalDocuments * 100);
            }

            var model = new PendingAndApprovedDocuments
            {
                ApprovedCountToday = approvedCountToday,
                PendingCount = pendingCount,
                ApprovedCount = approvedCount,
                ApprovalPercentage = approvalPercentage
            };

            return PartialView("_PendingAndApprovedDocuments", model);
        }

        [HttpGet]
        public async Task<IActionResult> GetTotalDocuments()
        {
            var today = DateTime.Today;
            var last7Days = Enumerable.Range(0, 7)
                .Select(i => today.AddDays(-i))
                .Reverse()
                .ToList();

            var documentsPerDay = new List<int>();
            foreach (var date in last7Days)
            {
                var count = await _context.Documents.CountAsync(d => d.UploadDate.Date <= date);
                documentsPerDay.Add(count);
            }

            var documentCount = await _context.Documents
                .CountAsync();

            var dates = last7Days.Select(d => d.ToString("dd/MM")).ToList();

            int firstDayCount = documentsPerDay.FirstOrDefault();
            int lastDayCount = documentsPerDay.LastOrDefault();

            double changePercentage = firstDayCount == 0 ?
                (lastDayCount > 0 ? 100 : 0) :
                ((double)(lastDayCount - firstDayCount) / firstDayCount) * 100;

            var model = new TotalDocumentViewModel
            {
                DocumentsPerDay = documentsPerDay,
                Dates = dates,
                DocumentCount = documentCount,
                ChangePercentage = changePercentage
            };

            return PartialView("_TotalDocuments", model);
        }

        [HttpGet]
        public async Task<IActionResult> GetRecentDocuments()
        {
            var model = await _context.Documents
                .Include(d => d.User)
                .Where(d => d.UploadDate.Date == DateTime.Today)
                .Take(5)
                .Select(d => new RecentDocumentViewModel
                {
                    Title = d.Title,
                    UploadDate = d.UploadDate,
                    IsApproved = d.IsApproved,
                    User = d.User,
                })
                .ToListAsync();

            return PartialView("_RecentDocuments", model);
        }

        [HttpGet]
        public async Task<IActionResult> GetTopContributorsToday()
        {
            var today = DateTime.Today;

            var groupedDocumentsToday = await _context.Documents
                .Where(d => d.UploadDate.Date == today)
                .GroupBy(d => d.UserId)
                .ToListAsync();

            var groupedDocumentsAllTime = await _context.Documents
               .GroupBy(d => d.UserId)
               .ToListAsync();

            var model = groupedDocumentsToday
                .Join(
                    _context.Users,
                    g => g.Key,
                    u => u.Id,
                    (g, u) => new TopContributorsTodayViewModel
                    {
                        User = u,
                        ContributionCountToday = g.Count(),
                        ContributionCountAllTime = groupedDocumentsAllTime
                        .FirstOrDefault(allTime => allTime.Key == g.Key)?
                        .Count() ?? 0
                    })
                .OrderByDescending(x => x.ContributionCountToday)
                .Take(4)
                .ToList();

            return PartialView("_TopContributorsToday", model);
        }

        [HttpGet]
        public async Task<IActionResult> GetTopDownloadedDocuments()
        {
            var model = await _context.Documents
                .Include(d => d.User)
                .OrderByDescending(d => d.Downloads)
                .Take(8)
                .ToListAsync();

            return PartialView("_TopDownloadedDocuments", model);
        }
            
        [HttpGet]
        public async Task<IActionResult> GetTopViewedDocuments()
        {
            var model = await _context.Documents
                .Include(d => d.User)
                .OrderByDescending(d => d.Views)
                .Take(8)
                .ToListAsync();

            return PartialView("_TopViewedDocuments", model);
        }
    }
}
