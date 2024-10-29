using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;

namespace StudyResource.Models
{
    public class User : IdentityUser
    {
        public required string FirstName { get; set; }
        public required string LastName { get; set; }
        public DateTime CreatedAt { get; set; }
        public virtual ICollection<Favorite>? Favorites { get; set; }
        public virtual ICollection<DownloadHistory>? DownloadHistories { get; set; }
        public virtual ICollection<UserComment> UserComments { get; set; } = new List<UserComment>();
    }
}
