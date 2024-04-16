namespace BIMS.Application.Services.BookCopies
{
    public interface IBookCopyService
    {
        BookCopy? Add(int bookId, int editionNumber, bool isAvailableForRental, string createdById);
        BookCopy? Update(int id, int editionNumber, bool isAvailableForRental, string updatedById);
        BookCopy? ToggleStatus(int id, string updatedById);
        BookCopy? GetDetails(int id);
        (string errorMessage, ICollection<RentalCopy> copies) CanBeRented(IEnumerable<int> selectedSerials, int subscriberId, int? rentalId = null);
        IEnumerable<BookCopy> GetRentalCopies(IEnumerable<int> copies);
        BookCopy? GetActiveCopyBySerialNumber(string serialNumber);
        bool CopyIsInRental(int id);

    }
}
