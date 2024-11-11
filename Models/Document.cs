namespace StudyResource.Models
{
    public class Document
    {
        public int Id { get; set; }
        public required string Title { get; set; }
        public required string Slug { get; set; }
        public required string Description { get; set; } = string.Empty;
        public int Views { get; set; }
        public int Downloads { get; set; }
        public required string GoogleDriveId { get; set; }
        public DateTime UploadDate { get; set; }
        public bool IsApproved { get; set; }
        public int GradeSubjectId { get; set; }
        public virtual GradeSubject? GradeSubject { get; set; }
        public int DocumentTypeId { get; set; }
        public virtual DocumentType? DocumentType { get; set; }
        public int? SetId { get; set; }
        public virtual Set? Set { get; set; }
        public string? UserId { get; set; }
        public virtual User? User { get; set; }
        public virtual ICollection<Favorite>? Favorite { get; set; }
        public virtual ICollection<DownloadHistory>? DownloadHistories { get; set; }
        public virtual ICollection<UserComment> UserComments { get; set; } = new List<UserComment>();
    }
}
