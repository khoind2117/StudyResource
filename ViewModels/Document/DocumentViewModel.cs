using System.Collections.Generic;
using StudyResource.Models;

namespace StudyResource.Models
{
    public class DocumentViewModel
    {
        public List<Document> Documents { get; set; }
        public List<GradeSubject> Grades { get; set; }
        public int? SelectedGrade { get; set; }
        public int SelectedDocumentType { get; set; }
        public string Query { get; set; }
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
    }
}