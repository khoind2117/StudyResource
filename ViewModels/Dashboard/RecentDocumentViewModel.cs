using StudyResource.Models;

namespace StudyResource.ViewModels.Dashboard
{
    public class RecentDocumentViewModel
    {
        public string? Title { get; set; }
        public DateTime UploadDate { get; set; }
        public bool IsApproved { get; set; }
        public User? User { get; set; }
    }

}
