using Aspose.Words;
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

                var fileExtension = Path.GetExtension(fileDownloadName).ToLower();
                if (fileExtension == ".pdf")
                {
                    return File(fileStream, "application/pdf", fileDownloadName, enableRangeProcessing: true);
                }
                else if (fileExtension == ".docx")
                {
                    var document = new Document(fileStream);
                    var pdfStream = new MemoryStream();
                    document.Save(pdfStream, SaveFormat.Pdf);
                    pdfStream.Position = 0;

                    return File(pdfStream, "application/pdf", fileDownloadName.Replace(".docx", ".pdf"), enableRangeProcessing: true);
                }
                else
                {
                    return BadRequest("Unsupported file type");
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal server error: " + ex.Message);
            }
        }
    }
}
