namespace StudyResource.Models
{
    public class UserComment
    {
        public int Id { get; set; }
        public string? UserId { get; set; }
        public virtual User? User { get; set; }
        public string Comment { get; set; }
        public int Rating { get; set; }
        public DateTime CommentDate { get; set; }
        public int DocumentId { get; set; }
        public virtual Document? Document { get; set; }
    }

}
