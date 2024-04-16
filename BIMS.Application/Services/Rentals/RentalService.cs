namespace BIMS.Application.Services.Rentals
{
	internal class RentalService : IRentalService
	{
		private readonly IUnitOfWork _unitOfWork;

		public RentalService(IUnitOfWork unitOfWork)
		{
			_unitOfWork = unitOfWork;
		}

		public IEnumerable<RentalCopy?> GetAllByCopyId(int bookCopyId)
		{
			var copyHistory = _unitOfWork.RentalCopies.FindAll(predicate: rc => rc.BookCopyId == bookCopyId,
				 include: c => c.Include(x => x.Rental)!.ThenInclude(x => x!.Subscriber)!,
				 orderBy: c => c.RentalDate,
				 orderByDirection: OrderBy.Descending);

			return copyHistory;
		}

		public IQueryable<Rental?> GetQueryableDetails(int id)
		{
			var query = _unitOfWork.Rentals.GetQueryable()
					  .Include(r => r.RentalCopies)
					  .ThenInclude(r => r.BookCopy)
					  .ThenInclude(r => r!.Book);
			return query;
		}

		public Rental Add(int subscriberId, ICollection<RentalCopy> copies, string createdById)
		{
			var rental = new Rental()
			{
				SubscriberId = subscriberId,
				RentalCopies = copies,
				CreatedById = createdById
			};

			_unitOfWork.Rentals.Add(rental);
			_unitOfWork.Commit();

			return rental;
		}
		public Rental? GetDetails(int id)
		{
			return _unitOfWork.Rentals.GetQueryable()
					.Include(r => r.RentalCopies)
					.ThenInclude(c => c.BookCopy)
					.SingleOrDefault(r => r.Id == id);
		}

		public Rental Update(int id, ICollection<RentalCopy> copies, string updatedById)
		{
			var rental = _unitOfWork.Rentals.GetById(id);

			rental!.RentalCopies = copies;
			rental.LastUpdatedById = updatedById;
			rental.LastUpdatedOn = DateTime.Now;

			_unitOfWork.Commit();

			return rental;
		}
		public bool AllowExtend(DateTime rentalStartDate, Subscriber subscriber)
		{
			return !subscriber.IsBlackListed
						&& subscriber!.Subscriptions.Last().EndDate >= rentalStartDate.AddDays((int)RentalConfigurations.MaxRentalDuration)
						&& rentalStartDate.AddDays((int)RentalConfigurations.RentalDuration) >= DateTime.Today;
		}
		public string? ValidateExtendedCopies(Rental rental, Subscriber subscriber)
		{
			string error = string.Empty;

			if (subscriber!.IsBlackListed)
				error = Errors.RentalNotAlloweForBlackListed;

			else if (subscriber!.Subscriptions.Last().EndDate < rental.StartDate.AddDays((int)RentalConfigurations.MaxRentalDuration))
				error = Errors.RentalNotAlloweForNotActive;

			else if (rental.StartDate.AddDays((int)RentalConfigurations.RentalDuration) < DateTime.Today)
				error = Errors.ExtendNotAllowed;

			return error;
		}
		public void Return(Rental rental, IList<ReturnCopyDto> copies, bool penaltyPaid, string updatedById)
		{
			var isUpdated = false;

			foreach (var copy in copies)
			{
				if (!copy.IsReturned.HasValue) continue;

				var currentCopy = rental.RentalCopies.SingleOrDefault(c => c.BookCopyId == copy.Id);

				if (currentCopy is null) continue;

				if (copy.IsReturned.HasValue && copy.IsReturned.Value)
				{
					if (currentCopy.ReturnDate.HasValue) continue;

					currentCopy.ReturnDate = DateTime.Now;
					isUpdated = true;
				}

				if (copy.IsReturned.HasValue && !copy.IsReturned.Value)
				{
					if (currentCopy.ExtendedOn.HasValue) continue;

					currentCopy.ExtendedOn = DateTime.Now;
					currentCopy.EndDate = currentCopy.RentalDate.AddDays((int)RentalConfigurations.MaxRentalDuration);
					isUpdated = true;
				}
			}

			if (isUpdated)
			{
				rental.LastUpdatedOn = DateTime.Now;
				rental.LastUpdatedById = updatedById;
				rental.PenaltyPaid = penaltyPaid;

				_unitOfWork.Commit();
			}
		}
		public int GetNumberOfCopies(int id)
		{
			return _unitOfWork.RentalCopies.Count(c => c.RentalId == id);
		}
		public Rental? MarkAsDeleted(int id, string deletedById)
		{
			var rental = _unitOfWork.Rentals.GetById(id);

			if (rental is null || rental.CreatedOn.Date != DateTime.Today)
				return null;

			rental.IsDeleted = true;
			rental.LastUpdatedOn = DateTime.Now;
			rental.LastUpdatedById = deletedById;

			_unitOfWork.Commit();

			return rental;
		}

	}
}
