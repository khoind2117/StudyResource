namespace StudyResource.ViewModels.Dashboard
{
    public class TotalDocumentViewModel
    {
        public List<int>? DocumentsPerDay { get; set; }
        public List<string>? Dates { get; set; }
        public int DocumentCount { get; set; }
        public int ChangePercentage { get; set; }
    }
}
