using StudyResource.Data;

namespace StudyResource
{
    public class Seed
    {
        private readonly ApplicationDbContext _context;
        public Seed(ApplicationDbContext context)
        {
            _context = context;   
        }

        public void SeedApplicationDbContext()
        {

        }
    }
}
