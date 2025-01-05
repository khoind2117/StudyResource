using CsvHelper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using StudyResource.Data;
using StudyResource.Models;
using StudyResource.Services;
using StudyResource.ViewModels.Image;
using StudyResource.ViewModels.Video;
using System.Globalization;
using System.Security.Claims;

namespace StudyResource.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class ImageController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly CloudinaryService _cloudinaryService;
        private readonly SlugService _slugService;

        public ImageController(ApplicationDbContext context,
            CloudinaryService cloudinaryService,
            SlugService slugService)
        {
            _context = context;
            _cloudinaryService = cloudinaryService;
            _slugService = slugService;
        }

        public async Task<IActionResult> Index()
        {
            var images = await _context.Images
                .Include(v => v.User)
                .Include(v => v.GradeSubject)
                .AsSplitQuery()
                .AsNoTracking()
                .ToListAsync();
            return View(images);
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
            var image = await _context.Images
                .Include(v => v.User)
                .Include(v => v.GradeSubject)
                   .ThenInclude(gs => gs.Grade)
                .Include(v => v.GradeSubject)
                   .ThenInclude(gs => gs.Subject)
                .AsSplitQuery()
                .AsNoTracking()
                .FirstOrDefaultAsync(v => v.Id == id);

            if (image == null)
            {
                return NotFound();
            }

            var viewModel = new AdminImageDetailViewModel
            {
                Id = id,
                PublicId = image.PublicId,
                Title = image.Title,
                Slug = image.Slug,
                Description = image.Description,
                Views = image.Views,
                Downloads = image.Downloads,
                Url = image.Url,
                ThumbnailUrl = image.ThumbnailUrl,
                DownloadUrl = image.DownloadUrl,
                FileSize = image.FileSize,
                Format = image.Format,
                UploadDate = image.UploadDate,
                IsApproved = image.IsApproved,
                GradeSubject = image.GradeSubject,
                User = image.User,
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
        public async Task<IActionResult> Create(CreateImageViewModel model)
        {
            if (ModelState.IsValid)
            {
                var uploadResult = await _cloudinaryService.UploadImageAsync(model.ImageUpload);

                if (uploadResult == null)
                {
                    ModelState.AddModelError("", "Có lỗi xảy ra khi tải hình ảnh lên.");
                    await PopulateSelectLists();
                    return View(model);
                }

                var slug = _slugService.GenerateSlug(model.Title);

                var image = new Image
                {
                    Title = model.Title,
                    PublicId = uploadResult.PublicId,
                    Description = model.Description,
                    Slug = slug,
                    Url = uploadResult.SecureUrl.ToString(),
                    ThumbnailUrl = $"{Path.ChangeExtension(uploadResult.SecureUrl.ToString().Split('?')[0], ".jpg")}",
                    DownloadUrl = uploadResult.SecureUrl.ToString().Replace("image/upload/", $"image/upload/fl_attachment:{slug}/"),
                    FileSize = uploadResult.Bytes,
                    Format = uploadResult.Format,
                    UploadDate = DateTime.Now,
                    IsApproved = User.IsInRole("Admin"),
                    GradeSubjectId = model.GradeSubjectId,
                    UserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value,
                };

                await _context.Images.AddAsync(image);
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Hình ảnh đã được tạo thành công!" });
            }

            return Json(new { success = false, message = "Dữ liệu không hợp lệ. Vui lòng kiểm tra lại." });
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var image = await _context.Images.FindAsync(id);
            if (image == null)
            {
                return Json(new { success = false, message = "Không tìm thấy hình ảnh." });
            }

            var deleteResult = await _cloudinaryService.DeleteImageAsync(image.PublicId);

            if (deleteResult.Result == "ok")
            {
                _context.Images.Remove(image);
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Hình ảnh đã được xóa thành công!" });
            }
            else
            {
                return Json(new { success = false, message = "Không thể xóa hình ảnh trên Cloudinary." });
            }
        }

        [HttpPost]
        public async Task<IActionResult> DeleteSelectedImages([FromBody] List<int> imageIds)
        {
            if (imageIds == null || !imageIds.Any())
            {
                return Json(new { success = false, message = "Không có video nào được chọn." });
            }

            try
            {
                foreach (var imageId in imageIds)
                {
                    var image = await _context.Images.FirstOrDefaultAsync(v => v.Id == imageId);
                    if (image != null)
                    {
                        var deleteResult = await _cloudinaryService.DeleteImageAsync(image.PublicId);
                        if (deleteResult.Result == "ok")
                        {
                            _context.Images.Remove(image);
                            await _context.SaveChangesAsync();
                        }
                        else
                        {
                            return Json(new { success = false, message = $"Không thể xóa hình ảnh có ID: {imageId} trên Cloudinary." });
                        }
                    }
                }

                return Json(new { success = true, message = "Các hình ảnh đã được xóa thành công." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Có lỗi xảy ra: {ex.Message}" });
            }
        }

        [HttpGet]
        public async Task<IActionResult> Update(int id)
        {
            var image = await _context.Images
                .Include(v => v.GradeSubject)
                    .ThenInclude(gs => gs.Grade)
                .AsSplitQuery()
                .FirstOrDefaultAsync(v => v.Id == id);

            if (image == null)
            {
                return NotFound();
            }

            var viewModel = new UpdateImageViewModel
            {
                Title = image.Title,
                Description = image.Description,
                PublicId = image.PublicId,
                Url = image.Url,
                GradeId = image.GradeSubject.GradeId,
                Grade = image.GradeSubject.Grade,
                GradeSubjectId = image.GradeSubjectId,
                GradeSubject = image.GradeSubject,
            };

            await PopulateSelectLists(image.GradeSubject.GradeId);
            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> Update(int id, UpdateImageViewModel model)
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

            var image = await _context.Images
                .FirstOrDefaultAsync(v => v.Id == id);

            if (image == null)
            {
                return NotFound();
            }

            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    var slug = _slugService.GenerateSlug(model.Title);
                    image.Title = model.Title;
                    image.Slug = slug;
                    image.Description = model.Description;
                    image.GradeSubjectId = model.GradeSubjectId;

                    if (model.ImageUpload != null && model.ImageUpload.Length > 0)
                    {
                        string newPublicId = null;

                        var uploadResult = await _cloudinaryService.UploadImageAsync(model.ImageUpload);

                        if (uploadResult.StatusCode == System.Net.HttpStatusCode.OK)
                        {
                            newPublicId = uploadResult.PublicId;
                            image.Url = uploadResult.SecureUrl.ToString();
                            image.ThumbnailUrl = $"{Path.ChangeExtension(uploadResult.SecureUrl.ToString().Split('?')[0], ".jpg")}";
                            image.DownloadUrl = uploadResult.SecureUrl.ToString().Replace("image/upload/", $"image/upload/fl_attachment:{slug}/");
                            image.FileSize = uploadResult.Bytes;
                            image.Format = uploadResult.Format;
                        }
                        else
                        {
                            return Json(new { success = false, message = "Không thể tải hình ảnh mới lên." });
                        }

                        if (!string.IsNullOrEmpty(image.PublicId) && newPublicId != null)
                        {
                            var deleteResult = await _cloudinaryService.DeleteImageAsync(image.PublicId);
                            if (deleteResult.Result != "ok")
                            {
                                return Json(new { success = false, message = "Không thể xóa hình ảnh cũ." });
                            }
                        }

                        image.PublicId = newPublicId;
                    }

                    await _context.SaveChangesAsync();

                    await transaction.CommitAsync();
                    return Json(new { success = true, message = "Cập nhật hình ảnh thành công." });
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
                            csv.Context.RegisterClassMap<ImageMap>();

                            var records = csv.GetRecords<ImageCsvViewModel>().ToList();

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
                                var url = _cloudinaryService.GenerateCloudinaryUrl(record.PublicId, "image");
                                var baseUrl = url.Split('?')[0];
                                var thumbnailUrl = baseUrl.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase)
                                    ? baseUrl
                                    : $"{baseUrl}.jpg";
                                var downloadUrl = baseUrl.Replace("image/upload/", $"image/upload/fl_attachment:{slug}/");

                                var image = new Image
                                {
                                    Title = record.Title,
                                    Slug = slug,
                                    Description = record.Description,
                                    PublicId = record.PublicId,
                                    Url = url,
                                    ThumbnailUrl = thumbnailUrl,
                                    DownloadUrl = downloadUrl,
                                    UploadDate = DateTime.Now,
                                    FileSize = 0,
                                    Format = "unknown",
                                    IsApproved = User.IsInRole("Admin"),
                                    GradeSubjectId = gradeSubject.Id,
                                    UserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value,
                                };

                                _context.Images.Add(image);
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
            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "csv-format-image.csv");

            if (!System.IO.File.Exists(filePath))
            {
                return NotFound();
            }

            var fileBytes = System.IO.File.ReadAllBytes(filePath);
            return File(fileBytes, "text/csv", "csv-format-image.csv");
        }
    }
}
