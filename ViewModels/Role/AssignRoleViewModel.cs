using Microsoft.AspNetCore.Identity;

namespace StudyResource.ViewModels.Role
{
    public class AssignRoleViewModel
    {
        public string UserId { get; set; }
        public List<IdentityRole> Roles { get; set; }
        public List<string> UserRoles { get; set; }
        public List<string> SelectedRoles { get; set; }
    }
}
