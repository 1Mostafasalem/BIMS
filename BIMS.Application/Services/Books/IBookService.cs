namespace BIMS.Application.Services.Books
{
    public interface IBookService
    {
        Book? GetById(int id);
        (IQueryable<Book> books, int count) GetFiltered(GetFilteredDto dto);
        IQueryable<Book> GetDetails();
        Book Add(Book book, string createdById);
        Book? GetWithCategories(int bookId);
        Book Update(Book book, string updatedById);
        bool AllowTitle(int id, string title, int authorId);
        Book? ToggleStatus(int id, string updatedById);
        IQueryable<Book> Search(string query);
    }
}
