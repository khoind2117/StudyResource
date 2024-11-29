namespace StudyResource.Models
{
    public class DocumentKeyword
    {
        public int Id { get; set; }

        public int DocumentId { get; set; }
        public virtual Document? Document { get; set; }

        public int KeywordId { get; set; }
        public virtual Keyword? Keyword { get; set; }
    }
}
