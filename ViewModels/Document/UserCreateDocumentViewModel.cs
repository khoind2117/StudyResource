using StudyResource.Models;

namespace StudyResource.ViewModels.Document
{
    public class UserCreateDocumentViewModel
    {
        public required string Title { get; set; }
        public required string Description { get; set; }
        public required IFormFile FileUpload { get; set; }

        public int GradeId { get; set; }
        public Grade? Grade { get; set; }

        public int GradeSubjectId { get; set; }
        public GradeSubject? GradeSubject { get; set; }

        public string? Keywords { get; set; }
    }
}
