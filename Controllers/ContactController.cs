using Microsoft.AspNetCore.Mvc;

namespace StudyResource.Controllers
{
    public class ContactController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
