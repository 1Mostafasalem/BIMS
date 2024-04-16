
namespace BIMS.Infrastructure.Persistence.Repositories
{
    internal class AuthorRepository : BaseRepository<Author>, IAuthorRepository
    {

        public AuthorRepository(ApplicationDbContext context) : base(context)
        {
        }
    }
}
