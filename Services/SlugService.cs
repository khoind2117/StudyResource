using StudyResource.Data;
using System.Text.RegularExpressions;

namespace StudyResource.Services
{
    public class SlugService
    {
        private readonly ApplicationDbContext _context;

        public SlugService(ApplicationDbContext context)
        {
            _context = context;
        }

        public string GenerateSlug(string title, int? id = null)
        {
            if (string.IsNullOrWhiteSpace(title))
            {
                return string.Empty;
            }

            string slug = title.ToLowerInvariant();
            slug = Regex.Replace(slug, @"[^a-z0-9\s-]", "");
            slug = Regex.Replace(slug, @"\s+", "-").Trim('-');

            // Kiểm tra slug có tồn tại trong cơ sở dữ liệu
            var existingSlugs = _context.Documents
                .Where(d => d.Slug == slug && (!id.HasValue || d.Id != id.Value))
                .Select(d => d.Slug)
                .ToList();

            // Nếu tồn tại, thêm số đếm hoặc ID vào slug
            if (existingSlugs.Any())
            {
                string originalSlug = slug;
                slug = $"{originalSlug}-{id}";
            }

            return slug;
        }
    }
}
