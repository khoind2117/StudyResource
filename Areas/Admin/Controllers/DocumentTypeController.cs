using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StudyResource.Data;
using StudyResource.Models;
using StudyResource.Services;
using StudyResource.ViewModels.DocumentType;
using System.Xml.Linq;

namespace StudyResource.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class DocumentTypeController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly SlugService _slugService;

        public DocumentTypeController(ApplicationDbContext context,
            SlugService slugService)
        {
            _context = context;
            _slugService = slugService;
        }

        [HttpGet]
        [Route("")]
        public async Task<IActionResult> IndexAsync()
        {
            var documentTypes = await _context.DocumentTypes
                .Include(dt => dt.Documents)
                .ToListAsync();
            return View(documentTypes);
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateDocumentTypeViewModel model)
        {
            if (ModelState.IsValid)
            {
                var documentType = new DocumentType
                {
                    Name = model.Name,
                    Slug = _slugService.GenerateSlug(model.Name)
                };

                _context.DocumentTypes.Add(documentType);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Update(int id)
        {
            var documentType = await _context.DocumentTypes.FindAsync(id);
            if (documentType == null)
            {
                return NotFound();
            }

            var model = new UpdateDocumentTypeViewModel
            {
                Name = documentType.Name
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Update(int id, UpdateDocumentTypeViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var documentType = await _context.DocumentTypes
                .FindAsync(id);

            if (documentType == null)
            {
                return NotFound();
            }

            documentType.Name = model.Name;

            _context.DocumentTypes.Update(documentType);
            await _context.SaveChangesAsync();

            return RedirectToAction("Index");
        }

        [HttpDelete]
        public async Task<IActionResult> Delete(int id)
        {
            var documentType = await _context.DocumentTypes.Include(dt => dt.Documents)
                .FirstOrDefaultAsync(dt => dt.Id == id);

            if (documentType == null)
            {
                return NotFound(new { success = false, message = "Không tìm thấy loại tài liệu." });
            }

            if (documentType.Documents.Any())
            {
                return BadRequest(new { success = false, message = "Không thể xóa loại tài liệu này vì vẫn còn tài liệu liên quan." });
            }

            _context.DocumentTypes.Remove(documentType);
            await _context.SaveChangesAsync();

            return Ok(new { success = true, message = "Loại tài liệu đã được xóa." });
        }
    }
}
