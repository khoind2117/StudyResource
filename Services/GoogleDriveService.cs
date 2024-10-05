using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Google.Apis.Services;
using Google.Apis.Upload;
using Google.Apis.Util.Store;

namespace StudyResource.Services
{
    public class GoogleDriveService
    {
        private readonly DriveService _driveService;
        public GoogleDriveService(IConfiguration configuration)
        {
            string[] scopes = { DriveService.Scope.Drive, DriveService.Scope.DriveFile };

            var clientId = configuration["GoogleDrive:ClientId"];
            var clientSecret = configuration["GoogleDrive:ClientSecret"];

            var credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
            new ClientSecrets
            {
                ClientId = clientId,
                ClientSecret = clientSecret
            },
            scopes,
            "user",
            CancellationToken.None).Result;

            _driveService = new DriveService(new BaseClientService.Initializer
            {
                HttpClientInitializer = credential,
                ApplicationName = "Study Resources"
            });
        }

        public async Task<string> UploadFileAsync(string path)
        {
            var fileMetadata = new Google.Apis.Drive.v3.Data.File
            {
                Name = Path.GetFileName(path)
            };

            using (var stream = new FileStream(path, FileMode.Open))
            {
                var request = _driveService.Files.Create(fileMetadata, stream, GetMimeType(path));
                request.Fields = "id";
                var result = await request.UploadAsync();

                if (result.Status == UploadStatus.Failed)
                {
                    throw new Exception($"Error uploading file: {result.Exception.Message}");
                }

                var fileId = request.ResponseBody.Id;
                var permission = new Google.Apis.Drive.v3.Data.Permission
                {
                    Role = "reader",
                    Type = "anyone"
                };
                var permissionRequest = _driveService.Permissions.Create(permission, fileId);
                await permissionRequest.ExecuteAsync();

                return fileId;
            }
        }

        public string GetDownloadLink(string fileId)
        {
            return $"https://drive.google.com/uc?export=download&id={fileId}";
        }

        public string GetViewLink(string fileId)
        {
            return $"https://drive.google.com/file/d/{fileId}/view";
        }

        public string GetPreviewLink(string fileId)
        {
            return $"https://drive.google.com/file/d/{fileId}/preview";
        }

        private string GetMimeType(string fileName)
        {
            var mimeType = "application/octet-stream";
            var ext = Path.GetExtension(fileName).ToLowerInvariant();
            switch (ext)
            {
                case ".pdf":
                    mimeType = "application/pdf";
                    break;
                case ".doc":
                case ".docx":
                    mimeType = "application/vnd.openxmlformats-officedocument.wordprocessingml.document";
                    break;
                case ".xls":
                case ".xlsx":
                    mimeType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                    break;
                case ".ppt":
                case ".pptx":
                    mimeType = "application/vnd.openxmlformats-officedocument.presentationml.presentation";
                    break;
            }
            return mimeType;
        }
    }
}
