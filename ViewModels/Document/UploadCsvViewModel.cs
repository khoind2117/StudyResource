using System.ComponentModel.DataAnnotations;

namespace StudyResource.ViewModels.Document
{
    public class UploadCsvViewModel
    {
        [Required]
        [Display(Name = "Tệp CSV")]
        public IFormFile CsvFile { get; set; }
    }
}
