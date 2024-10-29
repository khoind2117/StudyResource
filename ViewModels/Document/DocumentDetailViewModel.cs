using StudyResource.Models;

namespace StudyResource.ViewModels.Document
{
    public class DocumentDetailViewModel
    {
        public int Id { get; set; }
        public required string Title { get; set; }
        public required string Description { get; set; }
        public int DocumentTypeId { get; set; }
        public virtual Models.DocumentType? DocumentType { get; set; }
        public string? GoogleDriveId { get; set; }
        public string? UserNotes { get; set; }  
        public List<UserComment> UserComments { get; set; } = new List<UserComment>();

        public class UserComment
        {
            public string Username { get; set; } = string.Empty; 
            public string Comment { get; set; } = string.Empty; 
            public int Rating { get; set; }
            public DateTime CommentDate { get; set; } = DateTime.Now;
        }
    }
}
