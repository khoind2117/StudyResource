using StudyResource.Data;
using System.Globalization;
using System.Text;
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

        public string GenerateSlug(string title)
        {
            if (string.IsNullOrWhiteSpace(title))
            {
                return string.Empty;
            }

            title = title.ToLowerInvariant();

            // Xóa dấu
            title = RemoveDiacritics(title);

            // Thay ký tự đặc biệt tiếng Việt
            title = title.Replace("đ", "d").Replace("Đ", "d")
                         .Replace("á", "a").Replace("à", "a").Replace("ả", "a").Replace("ã", "a").Replace("ạ", "a")
                         .Replace("ắ", "a").Replace("ằ", "a").Replace("ẳ", "a").Replace("ẵ", "a").Replace("ặ", "a")
                         .Replace("â", "a").Replace("ấ", "a").Replace("ầ", "a").Replace("ẩ", "a").Replace("ẫ", "a").Replace("ậ", "a")
                         .Replace("é", "e").Replace("è", "e").Replace("ẻ", "e").Replace("ẽ", "e").Replace("ẹ", "e")
                         .Replace("ê", "e").Replace("ế", "e").Replace("ề", "e").Replace("ể", "e").Replace("ễ", "e").Replace("ệ", "e")
                         .Replace("ó", "o").Replace("ò", "o").Replace("ỏ", "o").Replace("õ", "o").Replace("ọ", "o")
                         .Replace("ô", "o").Replace("ố", "o").Replace("ồ", "o").Replace("ổ", "o").Replace("ỗ", "o").Replace("ộ", "o")
                         .Replace("ơ", "o").Replace("ớ", "o").Replace("ờ", "o").Replace("ở", "o").Replace("ỡ", "o").Replace("ợ", "o")
                         .Replace("ú", "u").Replace("ù", "u").Replace("ủ", "u").Replace("ũ", "u").Replace("ụ", "u")
                         .Replace("ư", "u").Replace("ứ", "u").Replace("ừ", "u").Replace("ử", "u").Replace("ữ", "u").Replace("ự", "u");

            // Thay thế ký tự gạch dưới thành gạch ngang
            title = title.Replace("_", "-");

            // Xóa ký tự đặc biệt (giữ lại 0-9, a-z, dấu gạch nối và khoảng trắng)
            title = Regex.Replace(title, @"[^0-9a-z-\s]", "");

            // Thay thế khoảng trắng thành dấu gạch nối
            title = Regex.Replace(title, @"\s+", "-");

            // Xóa ký tự gạch nối liên tiếp
            title = Regex.Replace(title, @"-+", "-");

            // Xóa dấu gạch nối ở đầu và cuối
            title = title.Trim('-');

            return title;
        }

        private string RemoveDiacritics(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return text;
            }

            // Chuyển đổi sang dạng unicode tổ hợp
            var normalizedString = text.Normalize(NormalizationForm.FormD);
            var stringBuilder = new StringBuilder();

            foreach (var c in normalizedString)
            {
                var unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c);
                if (unicodeCategory != UnicodeCategory.NonSpacingMark) // Loại bỏ các ký tự dấu
                {
                    stringBuilder.Append(c);
                }
            }

            return stringBuilder.ToString().Normalize(NormalizationForm.FormC); // Chuyển về dạng chuẩn
        }
    }
}
