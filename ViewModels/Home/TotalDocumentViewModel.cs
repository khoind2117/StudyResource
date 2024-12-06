namespace StudyResource.ViewModels.Home
{
    public class TotalDocumentViewModel
    {
        public List<int>? DocumentsPerDay { get; set; }
        public List<string>? Dates { get; set; }
        public int DocumentCount { get; set; }
        public double ChangePercentage { get; set; }
    }
}
