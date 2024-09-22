namespace StudyResource.Models
{
    public class Grade
    {
        public int Id { get; set; }
        public required string Name { get; set; }
        public required string Slug { get; set; }

        public virtual ICollection<Document>? Documents { get; set; }
    }
}