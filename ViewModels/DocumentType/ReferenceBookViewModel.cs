using StudyResource.Models;

namespace StudyResource.ViewModels.DocumentType
{
    public class ReferenceBookViewModel
    {
        public List<Models.Document>? SlickDocuments { get; set; }
        public List<Models.Document>? HighlyRatedDocuments { get; set; }
        public List<Grade>? Grades { get; set; }
    }
}
