using Microsoft.AspNetCore.Mvc;
using StudyResource.Models;
using System.Diagnostics;
using Microsoft.AspNetCore.Identity;
using StudyResource.Data;
using Microsoft.EntityFrameworkCore;


namespace StudyResource.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<HomeController> _logger;

        public HomeController(ApplicationDbContext context, ILogger<HomeController> logger)
        {
            _context = context;
            _logger = logger;
        }

        public IActionResult Index()
        {
            var documents = _context.Documents
                .Include(d => d.GradeSubject)         
                .Include(d => d.DocumentType)  
                .Take(6)                        
                .ToList();

            return View(documents);
        }

        public IActionResult Privacy()
        {

            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
