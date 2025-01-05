using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.Extensions.Options;
using System.Net.WebSockets;

namespace StudyResource.Services
{
    public class CloudinaryService
    {
        private readonly Cloudinary _cloudinary;
        private readonly string? _cloudName;

        public CloudinaryService(IConfiguration configuration)
        {
            _cloudName = configuration["CloudinarySettings:CloudName"];
            var apiKey = configuration["CloudinarySettings:ApiKey"];
            var apiSecret = configuration["CloudinarySettings:ApiSecret"];
        
            var acc = new Account(_cloudName, apiKey, apiSecret);
            _cloudinary = new Cloudinary(acc);
        }

        public string GenerateCloudinaryUrl(string publicId, string resourceType)
        {
            return $"https://res.cloudinary.com/{_cloudName}/{resourceType}/upload/{publicId}";
        }

        public async Task<VideoUploadResult> UploadVideoAsync(IFormFile file)
        {
            var uploadResult = new VideoUploadResult();
            if (file.Length > 0)
            {
                using var stream = file.OpenReadStream();
                var uploadParams = new VideoUploadParams
                {
                    File = new FileDescription(file.FileName, stream),
                };
                uploadResult = await _cloudinary.UploadAsync(uploadParams);
            }

            return uploadResult;
        }

        public async Task<DeletionResult> DeleteVideoAsync(string publicId)
        {
            try
            {
                var deleteParams = new DeletionParams(publicId)
                {
                    ResourceType = ResourceType.Video
                };
                var result = await _cloudinary.DestroyAsync(deleteParams);

                Console.WriteLine($"Delete Result: {result.Result}, PublicId: {publicId}");
                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting video: {ex.Message}");
                throw;
            }
        }

        public async Task<ImageUploadResult> UploadImageAsync(IFormFile file)
        {
            var uploadResult = new ImageUploadResult();
            if (file.Length > 0)
            {
                using var stream = file.OpenReadStream();
                var uploadParams = new ImageUploadParams
                {
                    File = new FileDescription(file.FileName, stream),
                };
                uploadResult = await _cloudinary.UploadAsync(uploadParams);
            }

            return uploadResult;
        }

        public async Task<DeletionResult> DeleteImageAsync(string publicId)
        {
            try
            {
                var deleteParams = new DeletionParams(publicId)
                {
                    ResourceType = ResourceType.Image
                };
                var result = await _cloudinary.DestroyAsync(deleteParams);

                Console.WriteLine($"Delete Result: {result.Result}, PublicId: {publicId}");
                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting image: {ex.Message}");
                throw;
            }
        }
    }
}
