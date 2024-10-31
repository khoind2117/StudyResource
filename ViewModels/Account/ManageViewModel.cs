using System.ComponentModel.DataAnnotations;

namespace StudyResource.ViewModels.Account
{
    public class ManageViewModel
    {
        public ProfileViewModel Profile { get; set; }
        public ChangePasswordViewModel ChangePassword { get; set; }
    }

    public class ProfileViewModel
    {
        public string UserName { get; set; }

        [DataType(DataType.EmailAddress)]
        [EmailAddress(ErrorMessage = "Email không hợp lệ.")]
        public string Email { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        [DataType(DataType.PhoneNumber)]
        [Phone(ErrorMessage = "Số điện thoại không hợp lệ.")]
        public string PhoneNumber { get; set; }
    }

    public class ChangePasswordViewModel
    {
        public string? OldPassword { get; set; }

        public string? NewPassword { get; set; }

        [Compare("NewPassword", ErrorMessage = "Mật khẩu và mật khẩu xác nhận không trùng nhau.")]
        public string? ConfirmPassword { get; set; }
    }

}