using BIMS.Application.Common.Interfaces.Repositories;

namespace BIMS.Application.Common.Interfaces
{
    public interface IUnitOfWork
    {
        IAuthorRepository Authors { get; }
        IAreaRepository Areas { get; }
        IBookRepository Books { get; }
        IBookCopyRepository BookCopies { get; }
        ICategoryRepository Categories { get; }
        IGovernoratesRepository Governorates{ get; }
        IRentalRepository Rentals { get; }
        IRentalCopyRepository RentalCopies { get; }
        ISubscriberRepository Subscribers { get; }
        ISubscriptionRepository Subscriptions { get; }
        int Commit();
    }
}
