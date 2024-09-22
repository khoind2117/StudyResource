namespace StudyResource.Models
{
    public class Favorite
    {
        public int Id { get; set; }

        public string? UserId { get; set; }
        public virtual User? User { get; set; }
        
        public int DocumentId { get; set; }
        public virtual Document? Document { get; set; }
    }
}