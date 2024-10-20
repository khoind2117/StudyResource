using System.ComponentModel.DataAnnotations;

namespace StudyResource.ViewModels.DocumentType
{
    public class CreateDocumentTypeViewModel
    {
        [Required(ErrorMessage = "Tên loại tài liệu là bắt buộc.")]
        [StringLength(30, ErrorMessage = "Tên loại tài liệu không được quá 30 ký tự.")]
        public required string Name { get; set; }
    }
}
