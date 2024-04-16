namespace BIMS.Infrastructure.Persistence.Repositories
{
    internal class SubscriberRepository : BaseRepository<Subscriber>, ISubscriberRepository
    {
        public SubscriberRepository(ApplicationDbContext context) : base(context)
        {
        }
    }
}
