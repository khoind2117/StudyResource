using CloudinaryDotNet.Core;
using CsvHelper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using StudyResource.Data;
using StudyResource.Models;
using StudyResource.Services;
using StudyResource.ViewModels.Document;
using StudyResource.ViewModels.Video;
using System.Globalization;
using System.Net.WebSockets;
using System.Security.Claims;

namespace StudyResource.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class VideoController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly CloudinaryService _cloudinaryService;
        private readonly SlugService _slugService;

        public VideoController(ApplicationDbContext context,
            CloudinaryService cloudinaryService,
            SlugService slugService)
        {
            _context = context;
            _cloudinaryService = cloudinaryService;
            _slugService = slugService;
        }

        public async Task<IActionResult> Index()
        {
            var videos = await _context.Videos
                .Include(v => v.User)
                .Include(v => v.GradeSubject)
                .AsSplitQuery()
                .AsNoTracking()
                .ToListAsync();
            return View(videos);
        }
        private async Task PopulateSelectLists(int? gradeId = null)
        {
            var grades = await _context.Grades.ToListAsync();
            var documentTypes = await _context.DocumentTypes.ToListAsync();
            var gradeSubjects = gradeId.HasValue
                ? await _context.GradeSubjects.Where(gs => gs.GradeId == gradeId.Value).ToListAsync()
                : new List<GradeSubject>();
            var sets = await _context.Sets.ToListAsync();

            ViewBag.Grades = new SelectList(grades, "Id", "Name");
            ViewBag.GradeSubjects = new SelectList(gradeSubjects, "Id", "Name");
            ViewBag.DocumentTypes = new SelectList(documentTypes, "Id", "Name");
            ViewBag.GradeSubjectsJson = await _context.GradeSubjects.ToListAsync();
            ViewBag.Sets = new SelectList(sets, "Id", "Name");
        }

        [HttpGet]
        public async Task<IActionResult> Detail(int id)
        {
            var video = await _context.Videos
                .Include(v => v.User)
                .Include(v => v.GradeSubject)
                   .ThenInclude(gs => gs.Grade)
                .Include(v => v.GradeSubject)
                   .ThenInclude(gs => gs.Subject)
                .AsSplitQuery()
                .AsNoTracking()
                .FirstOrDefaultAsync(v => v.Id == id);

            if (video == null)
            {
                return NotFound();
            }

            var viewModel = new AdminVideoDetailViewModel
            {
                Id = id,
                PublicId = video.PublicId,
                Title = video.Title,
                Slug = video.Slug,
                Description = video.Description,
                Views = video.Views,
                Downloads = video.Downloads,
                Url = video.Url,
                ThumbnailUrl = video.ThumbnailUrl,
                DownloadUrl = video.DownloadUrl,
                FileSize = video.FileSize,
                Format = video.Format,
                Duration = video.Duration,
                UploadDate = video.UploadDate,
                IsApproved = video.IsApproved,
                GradeSubject = video.GradeSubject,
                User = video.User,
            };

            return View(viewModel);
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            await PopulateSelectLists();
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateVideoViewModel model)
        {
            if (ModelState.IsValid)
            {
                var uploadResult = await _cloudinaryService.UploadVideoAsync(model.VideoUpload);
                
                if (uploadResult == null)
                {
                    ModelState.AddModelError("", "Có lỗi xảy ra khi tải video lên.");
                    await PopulateSelectLists();
                    return View(model);
                }

                var slug = _slugService.GenerateSlug(model.Title);

                var video = new Video
                {
                    Title = model.Title,
                    PublicId = uploadResult.PublicId,
                    Description = model.Description,
                    Slug = slug,
                    Url = uploadResult.SecureUrl.ToString(),
                    ThumbnailUrl = $"{Path.ChangeExtension(uploadResult.SecureUrl.ToString().Split('?')[0], ".jpg")}",
                    DownloadUrl = uploadResult.SecureUrl.ToString().Replace("video/upload/", $"video/upload/fl_attachment:{slug}/"),
                    FileSize = uploadResult.Bytes,
                    Format = uploadResult.Format,
                    Duration = uploadResult.Duration,
                    UploadDate = DateTime.Now,
                    IsApproved = User.IsInRole("Admin"),
                    GradeSubjectId = model.GradeSubjectId,
                    UserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value,
                };

                await _context.Videos.AddAsync(video);
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Video đã được tạo thành công!" });
            }

            return Json(new { success = false, message = "Dữ liệu không hợp lệ. Vui lòng kiểm tra lại." });
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var video = await _context.Videos.FindAsync(id);
            if (video == null)
            {
                return Json(new { success = false, message = "Không tìm thấy video." });
            }

            var deleteResult = await _cloudinaryService.DeleteVideoAsync(video.PublicId);

            if (deleteResult.Result == "ok")
            {
                _context.Videos.Remove(video);
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Video đã được xóa thành công!" });
            }
            else
            {
                return Json(new { success = false, message = "Không thể xóa video trên Cloudinary." });
            }
        }

        [HttpPost]
        public async Task<IActionResult> DeleteSelectedVideos([FromBody] List<int> videoIds)
        {
            if (videoIds == null || !videoIds.Any())
            {
                return Json(new { success = false, message = "Không có video nào được chọn." });
            }

            try
            {
                foreach (var videoId in videoIds)
                {
                    var video = await _context.Videos.FirstOrDefaultAsync(v => v.Id == videoId);
                    if (video != null)
                    {
                        var deleteResult = await _cloudinaryService.DeleteVideoAsync(video.PublicId);
                        if (deleteResult.Result == "ok")
                        {
                            _context.Videos.Remove(video);
                            await _context.SaveChangesAsync();
                        }
                        else
                        {
                            return Json(new { success = false, message = $"Không thể xóa video có ID: {videoId} trên Cloudinary." });
                        }
                    }
                }

                return Json(new { success = true, message = "Các video đã được xóa thành công." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Có lỗi xảy ra: {ex.Message}" });
            }
        }

        [HttpGet]
        public async Task<IActionResult> Update(int id)
        {
            var video = await _context.Videos
                .Include(v => v.GradeSubject)
                    .ThenInclude(gs => gs.Grade)
                .AsSplitQuery()
                .FirstOrDefaultAsync(v => v.Id == id);
            
            if (video == null)
            {
                return NotFound();
            }

            var viewModel = new UpdateVideoViewModel
            {
                Title = video.Title,
                Description = video.Description,
                PublicId = video.PublicId,
                Url = video.Url,
                GradeId = video.GradeSubject.GradeId,
                Grade = video.GradeSubject.Grade,
                GradeSubjectId = video.GradeSubjectId,
                GradeSubject = video.GradeSubject,
            };

            await PopulateSelectLists(video.GradeSubject.GradeId);
            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> Update(int id, UpdateVideoViewModel model)
        {
            if (!ModelState.IsValid)
            {
                //foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
                //{
                //    Console.WriteLine($"Error: {error.ErrorMessage}");
                //}

                await PopulateSelectLists(model.GradeId);
                return Json(new { success = false, message = "Vui lòng kiểm tra lại dữ liệu." });
            }

            var video = await _context.Videos
                .FirstOrDefaultAsync(v => v.Id == id);

            if (video == null)
            {
                return NotFound();
            }

            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    var slug = _slugService.GenerateSlug(model.Title);
                    video.Title = model.Title;
                    video.Slug = slug;
                    video.Description = model.Description;
                    video.GradeSubjectId = model.GradeSubjectId;

                    if (model.VideoUpload != null && model.VideoUpload.Length > 0)
                    {
                        string newPublicId = null;

                        var uploadResult = await _cloudinaryService.UploadVideoAsync(model.VideoUpload);

                        if (uploadResult.StatusCode == System.Net.HttpStatusCode.OK)
                        {
                            newPublicId = uploadResult.PublicId;
                            video.Url = uploadResult.SecureUrl.ToString();
                            video.ThumbnailUrl = $"{Path.ChangeExtension(uploadResult.SecureUrl.ToString().Split('?')[0], ".jpg")}";
                            video.DownloadUrl = uploadResult.SecureUrl.ToString().Replace("video/upload/", $"video/upload/fl_attachment:{slug}/");
                            video.Duration = uploadResult.Duration;
                            video.FileSize = uploadResult.Bytes;
                            video.Format = uploadResult.Format;
                        }
                        else
                        {
                            return Json(new { success = false, message = "Không thể tải video mới lên." });
                        }

                        if (!string.IsNullOrEmpty(video.PublicId) && newPublicId != null)
                        {
                            var deleteResult = await _cloudinaryService.DeleteVideoAsync(video.PublicId);
                            if (deleteResult.Result != "ok")
                            {
                                return Json(new { success = false, message = "Không thể xóa video cũ." });
                            }
                        }

                        video.PublicId = newPublicId;
                    }

                    await _context.SaveChangesAsync();

                    await transaction.CommitAsync();
                    return Json(new { success = true, message = "Cập nhật video thành công." });
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    return Json(new { success = false, message = $"Đã xảy ra lỗi: {ex.Message}" });
                }
            }
        }

        [HttpGet]
        public async Task<IActionResult> UploadCsv()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> UploadCsv(VideoUploadCsvViewModel model)
        {
            if (ModelState.IsValid)
            {
                var csvFile = model.CsvFile;
                if (csvFile != null && csvFile.Length > 0)
                {
                    try
                    {
                        using (var reader = new StreamReader(csvFile.OpenReadStream()))
                        using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
                        {
                            csv.Context.RegisterClassMap<VideoMap>();

                            var records = csv.GetRecords<VideoCsvViewModel>().ToList();

                            var errorList = new List<string>();
                            int rowIndex = 2;

                            foreach (var record in records)
                            {
                                if (string.IsNullOrWhiteSpace(record.Title) ||
                                    string.IsNullOrWhiteSpace(record.GradeSubjectName) ||
                                    string.IsNullOrWhiteSpace(record.PublicId))
                                {
                                    errorList.Add($"Hàng {rowIndex}: Một hoặc nhiều cột bị thiếu dữ liệu (Title, GradeSubjectName, PublicId hoặc Url).");
                                    rowIndex++;
                                    continue;
                                }

                                var gradeSubject = await _context.GradeSubjects
                                    .FirstOrDefaultAsync(gs => gs.Name.ToLower() == record.GradeSubjectName.ToLower());
                                if (gradeSubject == null)
                                {
                                    errorList.Add($"Hàng {rowIndex}: Môn học '{record.GradeSubjectName}' không tồn tại.");
                                    rowIndex++;
                                    continue;
                                }

                                var slug = _slugService.GenerateSlug(record.Title);
                                var url = _cloudinaryService.GenerateCloudinaryUrl(record.PublicId, "video");
                                var baseUrl = url.Split('?')[0];
                                var thumbnailUrl = baseUrl.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase)
                                    ? baseUrl
                                    : $"{baseUrl}.jpg";
                                var downloadUrl = baseUrl.Replace("video/upload/", $"video/upload/fl_attachment:{slug}/");

                                var video = new Video
                                {
                                    Title = record.Title,
                                    Slug = slug,
                                    Description = record.Description,
                                    PublicId = record.PublicId,
                                    Url = url,
                                    ThumbnailUrl = thumbnailUrl,
                                    DownloadUrl = downloadUrl,
                                    UploadDate = DateTime.Now,
                                    Duration = TimeSpan.Zero.TotalSeconds,
                                    FileSize = 0,
                                    Format = "unknown",
                                    IsApproved = User.IsInRole("Admin"),
                                    GradeSubjectId = gradeSubject.Id,
                                    UserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value,
                                };

                                _context.Videos.Add(video);
                                rowIndex++;
                            }

                            await _context.SaveChangesAsync();

                            if (errorList.Any())
                            {
                                TempData["ErrorList"] = errorList;
                            }
                            else
                            {
                                TempData["SuccessMessage"] = "Dữ liệu đã được nhập thành công!";
                            }

                            return RedirectToAction("UploadCsv");
                        }
                    }
                    catch (Exception ex)
                    {
                        ModelState.AddModelError("", $"Đã xảy ra lỗi: {ex.Message}. Vui lòng kiểm tra file CSV và thử lại.");
                        return View(model);
                    }
                }
                else
                {
                    ModelState.AddModelError("", "Vui lòng tải lên file CSV.");
                }
            }
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> DownloadCsv()
        {
            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "csv-format-video.csv");

            if (!System.IO.File.Exists(filePath))
            {
                return NotFound();
            }

            var fileBytes = System.IO.File.ReadAllBytes(filePath);
            return File(fileBytes, "text/csv", "csv-format-video.csv");
        }
    }
}
