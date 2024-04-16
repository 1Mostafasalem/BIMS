using BIMS.Domain.Entities;

namespace BIMS.Application.Services.Books
{
    internal class BookService : IBookService
    {
        private readonly IUnitOfWork _unitOfWork;

        public BookService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public Book? GetById(int id)
        {
            return _unitOfWork.Books.GetById(id);
        }
        public IQueryable<Book> GetDetails() => _unitOfWork.Books.GetDetails();


        public (IQueryable<Book> books, int count) GetFiltered(GetFilteredDto dto)
        {
            IQueryable<Book> books = _unitOfWork.Books.GetDetails();

            if (!string.IsNullOrEmpty(dto.SearchValue))
                books = books.Where(x => x.Title.Contains(dto.SearchValue!) || x.Author!.Name.Contains(dto.SearchValue!));

            books = books.OrderBy($"{dto.SortColumn} {dto.SortColumnDirection}")
                    .Skip(dto.Skip)
                    .Take(dto.PageSize);

            var recordsTotal = _unitOfWork.Books.Count();

            return (books, recordsTotal);
        }

        public Book Add(Book book, string createdById)
        {
            book.CreatedById = createdById;

            _unitOfWork.Books.Add(book);
            _unitOfWork.Commit();

            return book;
        }

        public Book? GetWithCategories(int bookId)
        {
            var book = _unitOfWork.Books.Find(predicate: x => x.Id == bookId,
                        include: b => b.Include(r => r.Categories).ThenInclude(c => c.Category!));
            return book;
        }

        public Book Update(Book book, string updatedById)
        {
            book.LastUpdatedById = updatedById;
            book.LastUpdatedOn = DateTime.Now;

            _unitOfWork.Commit();

            if (!book.IsAvilableForRental)
                _unitOfWork.BookCopies.SetAllAsNotAvailable(book.Id);

            return book;
        }

        public bool AllowTitle(int id, string title, int authorId)
        {
            var book = _unitOfWork.Books.Find(b => b.Title == title && b.AuthorId == authorId);
            return book is null || book.Id.Equals(id);
        }

        public Book? ToggleStatus(int id, string updatedById)
        {
            var book = _unitOfWork.Books.Find(b => b.Id == id);
            if (book is null)
                return null;

            book.IsDeleted = !book.IsDeleted;
            book.LastUpdatedById = updatedById;
            book.LastUpdatedOn = DateTime.Now;

            _unitOfWork.Commit();
            return book;
        }
        public IQueryable<Book> Search(string query)
        {
            return _unitOfWork.Books.GetQueryable()
                .Include(b => b.Author)
                .Where(b => !b.IsDeleted && (b.Title.Contains(query) || b.Author!.Name.Contains(query)
                || b.Publisher.Contains(query) || b.Categories.Any(c => c.Category!.Name.Contains(query))));
        }

    }
}
