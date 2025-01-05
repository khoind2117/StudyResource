namespace StudyResource.Models
{
    public class Video
    {
        public int Id { get; set; }
        public required string PublicId { get; set; }
        public required string Title { get; set; }
        public required string Slug { get; set; }
        public string Description { get; set; } = string.Empty;
        public int Views { get; set; }
        public int Downloads { get; set; }
        public required string Url { get; set; }
        public required string ThumbnailUrl { get; set; }
        public string? DownloadUrl { get; set; }
        public long FileSize { get; set; }
        public required string Format { get; set; }
        public double Duration { get; set; }
        public DateTime UploadDate { get; set; }
        public bool IsApproved { get; set; }
        public int GradeSubjectId { get; set; }
        public virtual GradeSubject? GradeSubject { get; set; }
        public string? UserId { get; set; }
        public virtual User? User { get; set; }
    }
}
