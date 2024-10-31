using Google.Apis.Drive.v3;
using Microsoft.AspNetCore.Mvc;
using StudyResource.Services;

namespace StudyResource.Controllers
{
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
                var fileDownloadName = await _googleDriveService.GetFileNameWithApiKeyAsync(fileId);
                if (fileDownloadName == null)
                {
                    return NotFound("File not found");
                }

                var fileStream = await _googleDriveService.DownloadFileWithApiKeyAsync(fileId);
                if (fileStream == null)
                {
                    return NotFound("File not found");
                }

                return File(fileStream, "application/pdf", fileDownloadName, enableRangeProcessing: true);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal server error: " + ex.Message);
            }
        }
    }
}
