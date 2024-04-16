namespace BIMS.Infrastructure.Persistence.Repositories
{
    internal class CategoryRepository : BaseRepository<Category>, ICategoryRepository
    {
        private readonly IApplicationDbContext _context;

        public CategoryRepository(ApplicationDbContext context) : base(context)
        {
        }
    }
}
