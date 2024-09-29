using Microsoft.AspNetCore.Mvc;

namespace StudyResource.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class Document : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
