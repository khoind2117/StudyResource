using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StudyResource.Data;
using StudyResource.Models;
using System.Linq;
using System.Threading.Tasks;

namespace StudyResource.Controllers
{
    [Authorize]
    public class FavoriteController : Controller
    {
        private readonly ApplicationDbContext _context;

        public FavoriteController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var userId = User.Claims.FirstOrDefault(c => c.Type == "UserId")?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            // Lấy danh sách tài liệu yêu thích của người dùng
            var favorites = await _context.Favorites
                .Where(f => f.UserId == userId)
                .Include(f => f.Document)
                .ToListAsync();

            return View(favorites);
        }

        [HttpPost]
        public async Task<IActionResult> AddToFavorite(int documentId)
        {
            var userId = User.Claims.FirstOrDefault(c => c.Type == "UserId")?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            // Kiểm tra xem tài liệu đã có trong danh sách yêu thích của người dùng chưa
            var existingFavorite = await _context.Favorites
                .FirstOrDefaultAsync(f => f.DocumentId == documentId && f.UserId == userId);

            if (existingFavorite != null)
            {
                return Json(new { success = false, message = "Document is already in favorites." });
            }

            // Thêm tài liệu vào danh sách yêu thích
            var favorite = new Favorite
            {
                DocumentId = documentId,
                UserId = userId
            };

            _context.Favorites.Add(favorite);
            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Document added to favorites." });
        }
    }
}
