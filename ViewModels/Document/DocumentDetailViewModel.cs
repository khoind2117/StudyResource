using StudyResource.Models;

namespace StudyResource.ViewModels.Document
{
    public class DocumentDetailViewModel
    {
        public int Id { get; set; }
        public required string Title { get; set; }
        public required string Slug { get; set; }
        public required string Description { get; set; }
        public int DocumentTypeId { get; set; }
        public virtual GradeSubject? GradeSubject { get; set; }
        public virtual Models.DocumentType? DocumentType { get; set; }
        public string? GoogleDriveId { get; set; }
        public List<DocumentKeyword>? DocumentKeywords { get; set; }
        public int Downloads { get; set; }
        public int Views { get; set; }
        public User? User { get; set; }
        public DateTime UploadDate { get; set; }
        public List<UserComment> UserComments { get; set; } = new List<UserComment>();
        public List<Models.Document> RelatedBooks { get; set; } = new List<Models.Document>(); 
        public int TotalComments { get; set; }
        public double AverageRating { get; set; }
        public class UserComment
        {
            public int Id { get; set; }
            public string Username { get; set; } = string.Empty; 
            public string Comment { get; set; } = string.Empty; 
            public int Rating { get; set; }
            public DateTime CommentDate { get; set; } = DateTime.Now;
        }
    }
}
