namespace StudyResource.ViewModels.Home
{
    public class AdminDashboardViewModel
    {
        public int PendingDocuments { get; set; }
        public List<int>? DownloadCount { get; set; }
        public List<int>? UploadCount { get; set; }
    }
}
