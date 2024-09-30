using System.ComponentModel.DataAnnotations;

namespace StudyResource.ViewModels.Account
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "Làm ơn điền email")]
        [EmailAddress]
        public string Email { get; set; }
        [Required(ErrorMessage = "Làm ơn điền mật khẩu")]
        [DataType(DataType.Password)]
        public string Password { get; set; }
        public string? ReturnUrl { get; set; }
    }
}
