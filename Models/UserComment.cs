using System.ComponentModel.DataAnnotations;

namespace StudyResource.Models
{
    public class UserComment
    {
        public int Id { get; set; }
        public string? UserId { get; set; }
        public virtual User? User { get; set; }
        [Required(ErrorMessage = "Bình luận không được để trống.")]
        [StringLength(500, ErrorMessage = "Bình luận không được vượt quá 500 ký tự.")]
        public string Comment { get; set; }
        [Range(1, 5, ErrorMessage = "Đánh giá phải nằm trong khoảng từ 1 đến 5.")]
        public int Rating { get; set; }
        public DateTime CommentDate { get; set; }
        public int DocumentId { get; set; }
        public virtual Document? Document { get; set; }
    }
}
