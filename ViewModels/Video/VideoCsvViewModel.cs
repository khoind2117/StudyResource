using CsvHelper.Configuration.Attributes;

namespace StudyResource.ViewModels.Video
{
    public class VideoCsvViewModel
    {
        [Name("Tiêu đề")]
        public string? Title { get; set; }
        [Name("Mô tả")]
        public string? Description { get; set; } = string.Empty;
        [Name("PublicId")]
        public string? PublicId { get; set; }
        [Name("Môn học + lớp")]
        public string? GradeSubjectName { get; set; }
    }
}
