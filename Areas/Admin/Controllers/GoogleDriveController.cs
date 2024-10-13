using Microsoft.AspNetCore.Mvc;
using StudyResource.Services;

namespace StudyResource.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class GoogleDriveController : Controller
    {
        private readonly GoogleDriveService _googleDriveService;

        public GoogleDriveController(GoogleDriveService googleDriveFileService)
        {
            _googleDriveService = googleDriveFileService;
        }

        [HttpGet]
        public async Task<IActionResult> DownloadFile(string fileId)
        {
            if (string.IsNullOrEmpty(fileId))
            {
                return BadRequest("File ID is required");
            }

            try
            {
                var fileStream = await _googleDriveService.DownloadFileWithApiKeyAsync(fileId);

                if (fileStream == null)
                {
                    return NotFound("File not found");
                }

                return File(fileStream, "application/pdf", "downloaded_file.pdf");
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal server error: " + ex.Message);
            }
        }
    }
}
