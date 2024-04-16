namespace BIMS.Infrastructure.Persistence.Repositories
{
    internal class AreaRepository : BaseRepository<Area>, IAreaRepository
    {
        public AreaRepository(ApplicationDbContext context) : base(context)
        {
        }
    }
}
