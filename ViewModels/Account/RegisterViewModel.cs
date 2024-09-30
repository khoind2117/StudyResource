using System.ComponentModel.DataAnnotations;

namespace StudyResource.ViewModels.Account
{
    public class RegisterViewModel
    {
        [Required(ErrorMessage = "Làm ơn điền họ của bạn")]
        public string LastName { get; set; }

        [Required(ErrorMessage = "Làm ơn điền tên của bạn")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "Làm ơn điền số điện thoại")]
        [DataType(DataType.PhoneNumber)]
        public string PhoneNumber { get; set; }

        [Required(ErrorMessage = "Làm ơn điền tài khoản email")]
        [DataType(DataType.EmailAddress)]
        public string Email { get; set; }

        [Required(ErrorMessage = "Làm ơn điền mật khẩu")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Mật khẩu phải chứa ít nhất 6 ký tự")]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [Required(ErrorMessage = "Làm ơn điền mật khẩu xác nhận")]
        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [Compare("Password", ErrorMessage = "Mật khẩu và mật khẩu xác nhận không trùng nhau")]
        public string ConfirmPassword { get; set; }
        public string? ReturnUrl { get; set; }
    }
}
