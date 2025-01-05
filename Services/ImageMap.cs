using CsvHelper.Configuration;
using StudyResource.ViewModels.Video;

namespace StudyResource.Services
{
    public class ImageMap : ClassMap<VideoCsvViewModel>
    {
        public ImageMap()
        {
            Map(m => m.Title).Name("Tiêu đề");
            Map(m => m.Description).Name("Mô tả");
            Map(m => m.PublicId).Name("PublicId");
            Map(m => m.GradeSubjectName).Name("Môn học + lớp");
        }
    }
}
