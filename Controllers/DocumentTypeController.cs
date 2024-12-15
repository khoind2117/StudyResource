using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StudyResource.Data;

namespace StudyResource.Controllers
{
    public class DocumentTypeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DocumentTypeController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            return View();
        }

        [HttpGet]        
        public async Task<IActionResult> ReferenceBook()
        {


            return View();
        }
    }
}
