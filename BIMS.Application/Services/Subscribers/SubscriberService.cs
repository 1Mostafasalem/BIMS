using BIMS.Domain.Enums;

namespace BIMS.Application.Services.Subscribers
{
    internal class SubscriberService : ISubscriberService
    {
        private readonly IUnitOfWork _unitOfWork;

        public SubscriberService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public Subscriber Add(Subscriber subscriber, string imagePath, string imageName, string createdById)
        {
            subscriber.ImageUrl = $"{imagePath}/{imageName}";
            subscriber.ImageThumbnailUrl = $"{imagePath}/thumb/{imageName}";
            subscriber.CreatedById = createdById;

            Subscription subscription = new()
            {
                CreatedById = subscriber.CreatedById,
                CreatedOn = subscriber.CreatedOn,
                StartDate = DateTime.Today,
                EndDate = DateTime.Today.AddYears(1)
            };

            subscriber.Subscriptions.Add(subscription);

            _unitOfWork.Subscribers.Add(subscriber);
            _unitOfWork.Commit();

            return subscriber;
        }

        public bool AllowEmail(int id, string email)
        {
            var subscriber = _unitOfWork.Subscribers.Find(c => c.Email == email);
            return subscriber is null || subscriber.Id.Equals(id);
        }

        public bool AllowMobileNumber(int id, string mobileNumber)
        {           
            var subscriber = _unitOfWork.Subscribers.Find(c => c.MobileNumber == mobileNumber);
            return subscriber is null || subscriber.Id.Equals(id);
        }

        public bool AllowNationalId(int id, string nationalId)
        {
            var subscriber = _unitOfWork.Subscribers.Find(c => c.NationalId == nationalId);
            return subscriber is null || subscriber.Id.Equals(id);
        }

        public Subscriber? GetById(int id) => _unitOfWork.Subscribers.GetById(id);

        public IQueryable<Subscriber> GetQueryable()
        {
            return _unitOfWork.Subscribers.GetQueryable();
        }

        public IQueryable<Subscriber> GetQueryableDetails()
        {
            var subscriber = _unitOfWork.Subscribers.GetQueryable()
                .Include(s => s.Governorate)
                .Include(s => s.Area)
                .Include(s => s.Subscriptions)
                .Include(s => s.Rentals)
                .ThenInclude(r => r.RentalCopies);

            return subscriber;
        }

        public Subscriber? GetSubscriberWithSubscriptions(int id)
        {
            var subscriber = _unitOfWork.Subscribers.Find(s => s.Id == id,
                include: s => s.Include(s => s.Subscriptions));

            return subscriber;
        }

        public Subscription RenewSubscription(int id, DateTime startDate, string createdById)
        {
            Subscription subscription = new()
            {
                SubscriberId = id,
                CreatedById = createdById,
                CreatedOn = DateTime.Now,
                StartDate = startDate,
                EndDate = startDate.AddYears(1),
            };

            _unitOfWork.Subscriptions.Add(subscription);
            _unitOfWork.Commit();

            return subscription;
        }

        public Subscriber? ToggleBlackListStatus(int id, string updatedById)
        {
            var subscriber = _unitOfWork.Subscribers.Find(b => b.Id == id);
            if (subscriber is null)
                return null;

            subscriber.IsBlackListed = !subscriber.IsBlackListed;
            subscriber.LastUpdatedById = updatedById;
            subscriber.LastUpdatedOn = DateTime.Now;

            _unitOfWork.Commit();
            return subscriber;
        }

        public void Update(Subscriber subscriber, string updatedById)
        {
            subscriber.LastUpdatedById = updatedById;
            subscriber.LastUpdatedOn = DateTime.Now;

            _unitOfWork.Commit();
        }

        public Subscriber? GetSubscriberWithRentals(int id)
        {
            return _unitOfWork.Subscribers.GetQueryable()
                    .Include(s => s.Subscriptions)
                    .Include(s => s.Rentals)
                    .ThenInclude(r => r.RentalCopies)
                    .SingleOrDefault(s => s.Id == id);
        }

        public (string errorMessage, int? maxAllowedCopies) CanRent(int id, int? rentalId = null)
        {
            var subscriber = GetSubscriberWithRentals(id);

            if (subscriber is null)
                return (errorMessage: Errors.NotFoundSubscriber, maxAllowedCopies: null);

            if (subscriber.IsBlackListed)
                return (errorMessage: Errors.BlackListedSubscriber, maxAllowedCopies: null);

            if (subscriber.Subscriptions.Last().EndDate < DateTime.Today.AddDays((int)RentalConfigurations.RentalDuration))
                return (errorMessage: Errors.InactiveSubscriber, maxAllowedCopies: null);

            var currentRentals = subscriber.Rentals
                .Where(r => rentalId == null || r.Id != rentalId)
                .SelectMany(r => r.RentalCopies)
                .Count(c => !c.ReturnDate.HasValue);

            var availableCopiesCount = (int)RentalConfigurations.MaxAllowedCopies - currentRentals;

            if (availableCopiesCount.Equals(0))
                return (errorMessage: Errors.MaxCopiesReaches, maxAllowedCopies: null);

            return (errorMessage: string.Empty, maxAllowedCopies: availableCopiesCount);
        }
    }
}
