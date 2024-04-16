namespace BIMS.Infrastructure.Persistence.Repositories
{
    internal class RentalCopyRepository : BaseRepository<RentalCopy>, IRentalCopyRepository
    {
        public RentalCopyRepository(ApplicationDbContext context) : base(context)
        {
        }
    }
}