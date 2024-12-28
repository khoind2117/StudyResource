using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StudyResource.Data;
using StudyResource.Models;
using StudyResource.Services;

namespace StudyResource.ViewComponents
{
    public class SidebarReferenceBookViewComponent : ViewComponent
    {
        private readonly ApplicationDbContext _context;

        public SidebarReferenceBookViewComponent(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IViewComponentResult> InvokeAsync(string filterType)
        {

            IEnumerable<Document> documents;

            switch (filterType)
            {
                case "latest":
                    documents = await _context.Documents
                                    .Where(d => d.IsApproved)
                                    .OrderByDescending(d => d.UploadDate)
                                    .Take(20)
                                    .ToListAsync();
                    break;
                case "mostDownloaded":
                    documents = await _context.Documents
                                    .Where(d => d.IsApproved)
                                    .OrderByDescending(d => d.Downloads)
                                    .Take(20)
                                    .ToListAsync();
                    break;
                default:
                    documents = await _context.Documents
                                    .Where(d => d.IsApproved)
                                    .OrderByDescending(d => d.UploadDate)
                                    .Take(20)
                                    .ToListAsync();
                    break;
            }

            return View(documents);
        }
    }
}
