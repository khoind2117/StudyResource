using CsvHelper.Configuration;
using StudyResource.ViewModels.Video;

namespace StudyResource.Services
{
    public class VideoMap : ClassMap<VideoCsvViewModel>
    {
        public VideoMap()
        {
            Map(v => v.Title).Name("Tiêu đề");
            Map(v => v.Description).Name("Mô tả");
            Map(v => v.PublicId).Name("PublicId");
            Map(v => v.GradeSubjectName).Name("Môn học + lớp");
        }
    }
}
