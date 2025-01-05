using StudyResource.Models;

namespace StudyResource.ViewModels.Image
{
    public class UpdateImageViewModel
    {
        public required string Title { get; set; }
        public required string Description { get; set; }
        public IFormFile? ImageUpload { get; set; }
        public string? PublicId { get; set; }
        public string? Url { get; set; }

        public int GradeId { get; set; }
        public Grade? Grade { get; set; }

        public int GradeSubjectId { get; set; }
        public GradeSubject? GradeSubject { get; set; }
    }
}
