using CsvHelper.Configuration.Attributes;

namespace StudyResource.ViewModels.Document
{
    public class DocumentCsvViewModel
    {
        [Name("Tiêu đề")]
        public string? Title { get; set; }
        [Name("Mô tả")]
        public string? Description { get; set; } = string.Empty;
        [Name("GoogleDriveId")]
        public string? GoogleDriveId { get; set; }
        [Name("Môn học + lớp")]
        public string? GradeSubjectName { get; set; }
        [Name("Loại tài liệu")]
        public string? DocumentTypeName { get; set; }
    }
}
