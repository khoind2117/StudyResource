using Microsoft.AspNetCore.Mvc;

namespace StudyResource.Controllers
{
    public class DocumentController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
