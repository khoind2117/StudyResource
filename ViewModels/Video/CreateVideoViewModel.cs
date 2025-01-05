using StudyResource.Models;

namespace StudyResource.ViewModels.Video
{
    public class CreateVideoViewModel
    {
        public required string Title { get; set; }
        public string Description { get; set; } = string.Empty;
        public required IFormFile VideoUpload { get; set; }

        public int GradeId { get; set; }
        public Grade? Grade { get; set; }
        public int GradeSubjectId { get; set; }
        public virtual GradeSubject? GradeSubject { get; set; }
        public string? UserId { get; set; }
        public virtual User? User { get; set; }
    }
}
