using System.ComponentModel.DataAnnotations;

namespace StudyResource.ViewModels.Video
{
    public class VideoUploadCsvViewModel
    {
        [Required]
        [Display(Name = "Tệp CSV")]
        public IFormFile CsvFile { get; set; }
    }
}
