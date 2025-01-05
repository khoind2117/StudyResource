using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StudyResource.Data;
using StudyResource.Models;
using X.PagedList.Extensions;

namespace StudyResource.Controllers
{
    public class ImageController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ImageController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(int? gradeId = 1, int? gradeSubjectId = 1, int page = 1, int pageSize = 12)
        {
            var imagesQuery = _context.Images
                .Include(v => v.GradeSubject)
                    .ThenInclude(gs => gs.Grade)
                .Include(v => v.GradeSubject)
                    .ThenInclude(gs => gs.Subject)
                .AsSplitQuery()
                .AsQueryable();

            if (gradeId.HasValue)
            {
                imagesQuery = imagesQuery.Where(v => v.GradeSubject.GradeId == gradeId);
            }

            if (gradeSubjectId.HasValue)
            {
                imagesQuery = imagesQuery.Where(v => v.GradeSubjectId == gradeSubjectId);
            }

            var totalImages = await imagesQuery.CountAsync();

            var images = imagesQuery
                .OrderByDescending(i => i.UploadDate)
                .ToPagedList(page, pageSize);

            if (!images.Any())
            {
                ViewBag.Message = $"Không tìm thấy hình ảnh nào.";
            }

            var grades = await _context.Grades.AsNoTracking().ToListAsync();
            var subjects = gradeId.HasValue
                ? await _context.Subjects
                    .Where(s => s.GradeSubjects.Any(gs => gs.GradeId == gradeId))
                    .AsNoTracking()
                    .ToListAsync()
                : new List<Subject>();

            ViewBag.Grades = grades;
            ViewBag.Subjects = subjects;
            ViewBag.SelectedGradeId = gradeId;
            ViewBag.SelectedGradeSubjectId = gradeSubjectId;
            ViewBag.GradeName = await _context.Grades
                .Where(g => g.Id == gradeId)
                .Select(g => g.Name)
                .AsNoTracking()
                .FirstOrDefaultAsync();

            ViewBag.GradeSubjectName = await _context.GradeSubjects
                .Where(gs => gs.Id == gradeSubjectId)
                .Select(gs => gs.Name)
                .AsNoTracking()
                .FirstOrDefaultAsync();
            ViewBag.Page = page;
            ViewBag.PageSize = pageSize;
            ViewBag.TotalImages = totalImages;

            return View(images);
        }

        [HttpGet]
        public async Task<IActionResult> GetSubjectsByGrade(int gradeId)
        {
            var subjects = await _context.GradeSubjects
                .Where(s => s.GradeId == gradeId)
                .ToListAsync();

            return Json(subjects);
        }

        [HttpGet]
        public async Task<IActionResult> Detail(int id)
        {
            var image = await _context.Images
                .Include(i => i.User)
                .Include(i => i.GradeSubject)
                    .ThenInclude(gs => gs.Grade)
                .AsSplitQuery()
                .AsNoTracking()
                .FirstOrDefaultAsync(i => i.Id == id);

            return View(image);
        }
    }
}
