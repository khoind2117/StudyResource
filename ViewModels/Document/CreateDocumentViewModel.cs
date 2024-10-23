using StudyResource.Models;

namespace StudyResource.ViewModels.Document
{
    public class CreateDocumentViewModel
    {
        public required string Title { get; set; }
        public required string Description { get; set; }
        public required IFormFile FileUpload { get; set; }

        public int GradeId { get; set; }
        public Grade? Grade { get; set; }

        public int GradeSubjectId { get; set; }
        public GradeSubject? GradeSubject { get; set; }

        public int DocumentTypeId { get; set; }
        public virtual Models.DocumentType? DocumentType { get; set; }

        public int? SetId { get; set; }
        public virtual Set? Set { get; set; }
    }
}
