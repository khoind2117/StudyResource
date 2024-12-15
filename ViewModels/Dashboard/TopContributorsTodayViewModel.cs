using StudyResource.Models;

namespace StudyResource.ViewModels.Dashboard
{
    public class TopContributorsTodayViewModel
    {
        public User? User { get; set; }
        public int ContributionCountToday { get; set; }
        public int ContributionCountAllTime { get; set; }
    }
}
