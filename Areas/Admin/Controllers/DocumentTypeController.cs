using Microsoft.AspNetCore.Mvc;
using StudyResource.Data;

namespace StudyResource.Areas.Admin.Controllers
{
    public class DocumentTypeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DocumentTypeController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            return View();
        }

        [HttpPut]
        public async Task<IActionResult> Update()
        {
            return View();
        }

        [HttpDelete]
        public async Task<IActionResult> Delete()
        {
            return RedirectToAction("Index");
        }
    }
}
