using CsvHelper.Configuration;
using StudyResource.Models;

namespace StudyResource.Services
{
    public class DocumentMap : ClassMap<Document>
    {
        public DocumentMap()
        {
            Map(m => m.Title).Name("Tiêu đề");
            Map(m => m.Description).Name("Mô tả");
            Map(m => m.GoogleDriveId).Name("GoogleDriveId");
            Map(m => m.GradeSubject).Name("Môn học + lớp");
            Map(m => m.DocumentType).Name("Loại tài liệu");
        }
    }
}
