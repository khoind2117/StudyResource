using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using System.Text;
using StudyResource.Models;
using Newtonsoft.Json;

namespace StudyResource.Controllers
{
    public class ContactController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> SubmitContactForm(ContactFormModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var client = new HttpClient();
            var json = JsonConvert.SerializeObject(model);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await client.PostAsync("https://script.google.com/macros/s/AKfycbws3ghnCygYHjgG1dUnQB8vwAqJ0t7gzi1yGZiHGQGu1EDT9Z7xJQSxyTcgL4pcLB67JQ/exec", content);

            if (response.IsSuccessStatusCode)
            {
                return Json(new { success = true, message = "Gửi thành công!" });
            }
            else
            {
                return Json(new { success = false, message = "Có lỗi xảy ra. Vui lòng thử lại." });
            }
        }
    }
}
