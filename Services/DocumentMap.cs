using CsvHelper.Configuration;
using StudyResource.Models;
using StudyResource.ViewModels.Document;

namespace StudyResource.Services
{
    public class DocumentMap : ClassMap<DocumentCsvViewModel>
    {
        public DocumentMap()
        {
            Map(m => m.Title).Name("Tiêu đề");
            Map(m => m.Description).Name("Mô tả");
            Map(m => m.GoogleDriveId).Name("GoogleDriveId");
            Map(m => m.GradeSubjectName).Name("Môn học + lớp");
            Map(m => m.DocumentTypeName).Name("Loại tài liệu");
            Map(m => m.SetName).Name("Bộ sách");
            Map(m => m.Keyword).Name("Từ khóa");
        }
    }
}
