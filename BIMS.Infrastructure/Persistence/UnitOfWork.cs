using BIMS.Application.Common.Interfaces.Repositories;

namespace BIMS.Infrastructure.Persistence
{
    internal class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext _context;

        public UnitOfWork(ApplicationDbContext context) => _context = context;

        public IAuthorRepository Authors => new AuthorRepository(_context);
        public IAreaRepository Areas => new AreaRepository(_context);
        public IBookRepository Books => new BookRepository(_context);
        public IBookCopyRepository BookCopies => new BookCopyRepository(_context);
        public ICategoryRepository Categories => new CategoryRepository(_context);
        public IGovernoratesRepository Governorates =>  new GovernoratesRepository(_context);
        public IRentalRepository Rentals => new RentalRepository(_context);
        public IRentalCopyRepository RentalCopies => new RentalCopyRepository(_context);
        public ISubscriberRepository Subscribers => new SubscriberRepository(_context);
        public ISubscriptionRepository Subscriptions => new SubscriptionRepository(_context);

        public int Commit() => _context.SaveChanges();
    }
}
