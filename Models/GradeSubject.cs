namespace StudyResource.Models
{
    public class GradeSubject
    {
        public int Id { get; set; }
        public required string Name { get; set; }
        public required string Slug { get; set; }

        public int GradeId { get; set; }
        public Grade? Grade { get; set; }
    
        public int SubjectId { get; set; }
        public Subject? Subject { get; set; }

        public virtual ICollection<Document>? Documents { get; set; }
    }
}
