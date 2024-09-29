namespace StudyResource.Models
{
    public class Subject
    {
        public int Id { get; set; }
        public required string Name { get; set; }
        public required string Slug { get; set; }

        public virtual ICollection<GradeSubject>? GradeSubjects { get; set; }
    }
}