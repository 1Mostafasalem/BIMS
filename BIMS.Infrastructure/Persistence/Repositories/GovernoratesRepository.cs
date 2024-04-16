namespace BIMS.Infrastructure.Persistence.Repositories
{
    internal class GovernoratesRepository : BaseRepository<Governorate>, IGovernoratesRepository
    {
        public GovernoratesRepository(ApplicationDbContext context) : base(context)
        {
        }
    }
}
