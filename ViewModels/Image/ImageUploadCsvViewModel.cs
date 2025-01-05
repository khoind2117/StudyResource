using System.ComponentModel.DataAnnotations;

namespace StudyResource.ViewModels.Image
{
    public class ImageUploadCsvViewModel
    {
        [Required]
        [Display(Name = "Tệp CSV")]
        public IFormFile CsvFile { get; set; }
    }
}
