namespace StudyResource.Models
{
    public class Document
    {
        public int Id { get; set; }
        public required string Title { get; set; }
        public required string Slug { get; set; }
        public required string Description { get; set; }
        public int Views { get; set; }
        public int Downloads { get; set; }
        public required string FilePath { get; set; }
        public DateTime UploadDate { get; set; }

        public int SubjectId { get; set; }
        public virtual Subject? Subject { get; set; }

        public int GradeId { get; set; }
        public virtual Grade? Grade { get; set; }

        public int DocumentTypeId { get; set; }
        public virtual DocumentType? DocumentType { get; set; }

        public virtual ICollection<Favorite>? Favorite { get; set; }
        public virtual ICollection<DownloadHistory>? DownloadHistories { get; set; }
    }
}
