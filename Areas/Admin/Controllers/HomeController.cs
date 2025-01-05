using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StudyResource.Data;
using StudyResource.ViewModels.Dashboard;

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
            var sevenDaysAgo = today.AddDays(-6);

            var documentUploads = await _context.Documents
                .Where(d => d.UploadDate >= sevenDaysAgo && d.UploadDate <= today)
                .GroupBy(d => d.UploadDate.Date)
                .Select(g => new { Date = g.Key, Count = g.Count() })
                .AsNoTracking()
                .ToListAsync();

            var downloadHistories = await _context.DownloadHistories
                .Where(d => d.DownloadDate >= sevenDaysAgo && d.DownloadDate <= today)
                .GroupBy(d => d.DownloadDate.Date)
                .Select(g => new { Date = g.Key, Count = g.Count() })
                .AsNoTracking()
                .ToListAsync();

            var days = Enumerable.Range(0, 7)
                .Select(i => today.AddDays(-i).Date)
                .Reverse()
                .ToList();

            var uploadDict = documentUploads.ToDictionary(u => u.Date, u => u.Count);
            var downloadDict = downloadHistories.ToDictionary(d => d.Date, d => d.Count);

            var uploadCount = days.Select(day => uploadDict.GetValueOrDefault(day, 0)).ToList();
            var downloadCount = days.Select(day => downloadDict.GetValueOrDefault(day, 0)).ToList();

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

            var documentCounts = await _context.Documents
                .GroupBy(d => new
                {
                    IsApproved = d.IsApproved,
                    IsToday = d.UploadDate.Date == today
                })
                .Select(g => new
                {
                    g.Key.IsApproved,
                    g.Key.IsToday,
                    Count = g.Count()
                })
                .ToListAsync();

            var approvedCountToday = documentCounts
                .Where(d => d.IsApproved && d.IsToday)
                .Sum(d => d.Count);

            var pendingCount = documentCounts
                .Where(d => !d.IsApproved)
                .Sum(d => d.Count);

            var approvedCount = documentCounts
                .Where(d => d.IsApproved)
                .Sum(d => d.Count);

            var totalDocuments = approvedCount + pendingCount;
            int approvalPercentage = totalDocuments > 0
                ? (int)((double)approvedCount / totalDocuments * 100)
                : 0;

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
                ChangePercentage = (int)Math.Round(changePercentage)
            };

            return PartialView("_TotalDocuments", model);
        }

        [HttpGet]
        public async Task<IActionResult> GetRecentDocuments()
        {
            var model = await _context.Documents
                .Include(d => d.User)
                .OrderByDescending(d => d.UploadDate)
                .Take(5)
                .Select(d => new RecentDocumentViewModel
                {
                    Title = d.Title,
                    UploadDate = d.UploadDate,
                    IsApproved = d.IsApproved,
                    User = d.User,
                })
                .AsNoTracking()
                .ToListAsync();

            return PartialView("_RecentDocuments", model);
        }

        [HttpGet]
        public async Task<IActionResult> GetTopContributorsToday()
        {
            var today = DateTime.Today;

            var contributions = await _context.Documents
                .GroupBy(d => new { d.UserId, IsToday = d.UploadDate.Date == today })
                .Select(g => new
                {
                    UserId = g.Key.UserId,
                    IsToday = g.Key.IsToday,
                    Count = g.Count()
                })
                .ToListAsync();

            var contributionsToday = contributions
                .Where(c => c.IsToday)
                .ToDictionary(c => c.UserId, c => c.Count);

            var contributionsAllTime = contributions
                .GroupBy(c => c.UserId)
                .ToDictionary(g => g.Key, g => g.Sum(c => c.Count));

            var userIds = contributionsToday.Keys
                .Concat(contributionsAllTime.Keys)
                .Distinct()
                .ToList();

            var users = await _context.Users
                .AsNoTracking()
                .Where(u => userIds.Contains(u.Id))
                .ToListAsync();

            var model = users
                .Select(u => new TopContributorsTodayViewModel
                {
                    User = u,
                    ContributionCountToday = contributionsToday.ContainsKey(u.Id) ? contributionsToday[u.Id] : 0,
                    ContributionCountAllTime = contributionsAllTime.ContainsKey(u.Id) ? contributionsAllTime[u.Id] : 0
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
                .Take(12)
                .AsNoTracking()
                .ToListAsync();

            return PartialView("_TopDownloadedDocuments", model);
        }
            
        [HttpGet]
        public async Task<IActionResult> GetTopViewedDocuments()
        {
            var model = await _context.Documents
                .Include(d => d.User)
                .OrderByDescending(d => d.Views)
                .Take(12)
                .AsNoTracking()
                .ToListAsync();

            return PartialView("_TopViewedDocuments", model);
        }
    }
}
