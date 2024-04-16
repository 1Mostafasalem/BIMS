namespace BIMS.Application.Services.Subscribers
{
    public interface ISubscriberService
    {
        Subscriber? GetById(int id);
        IQueryable<Subscriber> GetQueryable();
        Subscriber Add(Subscriber subscriber, string imagePath, string imageName, string createdById);
        void Update(Subscriber subscriber, string updatedById);
        IQueryable<Subscriber> GetQueryableDetails();
        bool AllowNationalId(int id, string nationalId);
        bool AllowMobileNumber(int id, string mobileNumber);
        bool AllowEmail(int id, string email);
        Subscriber? GetSubscriberWithSubscriptions(int id);
        Subscription RenewSubscription(int id, DateTime startDate, string createdById);
        Subscriber? ToggleBlackListStatus(int id, string updatedById);
        (string errorMessage, int? maxAllowedCopies) CanRent(int id, int? rentalId = null);

    }
}
