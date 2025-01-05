using StudyResource.Models;

namespace StudyResource.ViewModels.Video
{
    public class UpdateVideoViewModel
    {
        public required string Title { get; set; }
        public required string Description { get; set; }
        public IFormFile? VideoUpload { get; set; }
        public string? PublicId { get; set; }
        public string? Url { get; set; }

        public int GradeId { get; set; }
        public Grade? Grade { get; set; }

        public int GradeSubjectId { get; set; }
        public GradeSubject? GradeSubject { get; set; }
    }
}
