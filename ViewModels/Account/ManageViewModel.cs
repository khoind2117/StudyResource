using System.ComponentModel.DataAnnotations;

namespace StudyResource.ViewModels.Account
{
    public class ManageViewModel
    {
        /*        public ProfileViewModel Manage { get; set; } = new ProfileViewModel();
                public ChangePasswordViewModel ChangePassword { get; set; } = new ChangePasswordViewModel();*/
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

/*    public class ProfileViewModel
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
        [DataType(DataType.Password)]
        [Required(ErrorMessage = "Mật khẩu hiện tại không được để trống.", AllowEmptyStrings = false)]
        public string OldPassword { get; set; }

        [DataType(DataType.Password)]
        [Required(ErrorMessage = "Mật khẩu mới không được để trống.", AllowEmptyStrings = false)]
        [StringLength(100, ErrorMessage = "Mật khẩu mới phải có ít nhất {2} ký tự.", MinimumLength = 6)]
        public string NewPassword { get; set; }

        [DataType(DataType.Password)]
        [Compare("NewPassword", ErrorMessage = "Mật khẩu xác nhận không khớp.")]
        public string ConfirmPassword { get; set; }
    }*/
}
