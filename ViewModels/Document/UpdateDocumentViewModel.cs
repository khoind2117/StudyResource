using StudyResource.Models;

namespace StudyResource.ViewModels.Document
{
    public class UpdateDocumentViewModel
    {
        public required string Title { get; set; }
        public required string Description { get; set; }
        public IFormFile? FileUpload { get; set; }
        public string? GoogleDriveId { get; set; }

        public int GradeId { get; set; }
        public Grade? Grade { get; set; }

        public int GradeSubjectId { get; set; }
        public GradeSubject? GradeSubject { get; set; }

        public int DocumentTypeId { get; set; }
        public virtual DocumentType? DocumentType { get; set; }
    }
}
