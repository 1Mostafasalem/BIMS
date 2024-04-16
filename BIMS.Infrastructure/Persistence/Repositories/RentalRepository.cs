namespace BIMS.Infrastructure.Persistence.Repositories
{
    internal class RentalRepository : BaseRepository<Rental>, IRentalRepository
    {
        public RentalRepository(ApplicationDbContext context) : base(context)
        {
        }
    }
}
