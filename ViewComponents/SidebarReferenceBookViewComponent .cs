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

            var documentTypeId = await _context.DocumentTypes
               .Where(d => d.Name.ToLower() == "tài liệu tham khảo")
               .Select(d => d.Id)
               .FirstOrDefaultAsync();

            switch (filterType)
            {
                case "latest":
                    documents = await _context.Documents
                                    .Where(d => d.IsApproved && d.DocumentTypeId == documentTypeId)
                                    .OrderByDescending(d => d.UploadDate)
                                    .Take(10)
                                    .AsNoTracking()
                                    .ToListAsync();
                    break;
                case "mostDownloaded":
                    documents = await _context.Documents
                                    .Where(d => d.IsApproved && d.DocumentTypeId == documentTypeId)
                                    .OrderByDescending(d => d.Downloads)
                                    .Take(10)
                                    .AsNoTracking()
                                    .ToListAsync();
                    break;
                default:
                    documents = await _context.Documents
                                    .Where(d => d.IsApproved && d.DocumentTypeId == documentTypeId)
                                    .OrderByDescending(d => d.UploadDate)
                                    .Take(10)
                                    .AsNoTracking()
                                    .ToListAsync();
                    break;
            }

            return View(documents);
        }
    }
}
