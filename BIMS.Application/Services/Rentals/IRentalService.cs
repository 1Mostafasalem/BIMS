namespace BIMS.Application.Services.Rentals
{
    public interface IRentalService
    {
        Rental? GetDetails(int id);
        IEnumerable<RentalCopy?> GetAllByCopyId(int bookCopyId);
        IQueryable<Rental?> GetQueryableDetails(int id);
        Rental Add(int subscriberId, ICollection<RentalCopy> copies, string createdById);
        Rental Update(int id, ICollection<RentalCopy> copies, string updatedById);
        bool AllowExtend(DateTime rentalStartDate, Subscriber subscriber);
		string? ValidateExtendedCopies(Rental rental, Subscriber subscriber);
        void Return(Rental rental, IList<ReturnCopyDto> copies, bool penaltyPaid, string updatedById);
        Rental? MarkAsDeleted(int id, string deletedById);
        int GetNumberOfCopies(int id);

	}
}
