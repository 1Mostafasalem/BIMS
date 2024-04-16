
using BIMS.Application.Common.Interfaces;

namespace BIMS.Infrastructure.Persistence.Repositories
{
    internal class BookRepository : BaseRepository<Book>, IBookRepository
    {

        public BookRepository(ApplicationDbContext context) : base(context)
        {
        }

        public IQueryable<Book> GetDetails()
        {
            var query = _context.Books
                    .Include(a => a.Author)
                    .Include(b => b.Copies)
                    .Include(b => b.Categories)
                    .ThenInclude(c => c.Category);

            return query;
        }
    }
}
