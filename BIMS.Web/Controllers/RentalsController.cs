namespace BIMS.Web.Controllers
{
	[Authorize(Roles = AppRoles.Reception)]
	public class RentalsController : Controller
	{
		private readonly IMapper _mapper;
		private readonly IDataProtector _dataProtector;
		private readonly IBookCopyService _bookCopyService;
		private readonly ISubscriberService _subscriberService;
		private readonly IRentalService _rentalService;


		public RentalsController(IMapper mapper, IDataProtectionProvider dataProtector, IRentalService rentalService, ISubscriberService subscriberService, IBookCopyService bookCopyService)
		{
			_mapper = mapper;
			_dataProtector = dataProtector.CreateProtector("SecureKey");
			_rentalService = rentalService;
			_subscriberService = subscriberService;
			_bookCopyService = bookCopyService;
		}

		public IActionResult Create(string sKey)
		{
			var subscriberId = int.Parse(_dataProtector.Unprotect(sKey));

			var (errorMessage, maxAllowedCopies) = _subscriberService.CanRent(subscriberId);

			if (!string.IsNullOrEmpty(errorMessage))
				return View("NotAllowedRental", errorMessage);

			var viewModel = new RentalFormViewModel
			{
				SubscriberKey = sKey,
				MaxAllowedCopies = maxAllowedCopies
			};

			return View("Form", viewModel);
		}


		[HttpPost]
		[ValidateAntiForgeryToken]
		public IActionResult Create(RentalFormViewModel model)
		{
			if (!ModelState.IsValid)
				return View("Form", model);

			var subscriberId = int.Parse(_dataProtector.Unprotect(model.SubscriberKey));

			var (errorMessage, maxAllowedCopies) = _subscriberService.CanRent(subscriberId);

			if (!string.IsNullOrEmpty(errorMessage))
				return View("NotAllowedRental", errorMessage);

			var (rentalsError, copies) = _bookCopyService.CanBeRented(model.SelectedCopies, subscriberId);

			if (!string.IsNullOrEmpty(rentalsError))
				return View("NotAllowedRental", rentalsError);

			var rental = _rentalService.Add(subscriberId, copies, User.GetUserId());

			//return RedirectToAction(nameof(Details), new { id = rental.Id });
			return RedirectToAction(nameof(Details), new { rKey = _dataProtector.Protect(rental.Id.ToString()) });
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public IActionResult GetCopyDetails(SubscriberSearchViewModel model)
		{
			if (!ModelState.IsValid)
				return BadRequest();

			var copy = _bookCopyService.GetActiveCopyBySerialNumber(model.Value);

			if (copy is null)
				return NotFound(Errors.InvalidSerialNumber);

			if (!copy.IsAvailableForRental || !copy.Book!.IsAvilableForRental)
				return BadRequest(Errors.NotAvailableRental);

			//Check that copy is in rental 

			var copyIsInRental = _bookCopyService.CopyIsInRental(copy.Id);

			if (copyIsInRental)
				return BadRequest(Errors.CopyIsInRental);

			BookCopyViewModel viewModel = _mapper.Map<BookCopyViewModel>(copy);

			return PartialView("_CopyDetails", viewModel);
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public IActionResult MarkAsDeleted(string rKey)
		{
			var rentalId = int.Parse(_dataProtector.Unprotect(rKey));
			var rental = _rentalService.MarkAsDeleted(rentalId, User.GetUserId());

			if (rental is null)
				return NotFound();

			var copiesCount = _rentalService.GetNumberOfCopies(rentalId);

			return Ok(copiesCount);
		}

		public IActionResult Details(string rKey)
		{
			var rentalId = int.Parse(_dataProtector.Unprotect(rKey));

			var query = _rentalService.GetQueryableDetails(rentalId);

			var viewModel = _mapper.ProjectTo<RentalViewModel>(query).SingleOrDefault(r => r.Id == rentalId);
			if (viewModel is null)
				return NotFound();

			viewModel.Key = rKey;
			return View(viewModel);
		}

		public IActionResult Edit(string rKey)
		{
			var rentalId = int.Parse(_dataProtector.Unprotect(rKey));

			var rental = _rentalService.GetDetails(rentalId);


			if (rental is null || rental.CreatedOn.Date != DateTime.Today)
				return NotFound();

			var (errorMessage, availableCopiesCount) = _subscriberService.CanRent(rental.SubscriberId, rental.Id);

			if (!string.IsNullOrEmpty(errorMessage))
				return View("NotAllowedRental", errorMessage);

			var currentCopiesIds = rental.RentalCopies.Select(c => c.BookCopyId).ToList();

			var currentCopies = _bookCopyService.GetRentalCopies(currentCopiesIds);


			var viewModel = new RentalFormViewModel
			{
				SubscriberKey = _dataProtector.Protect(rental.SubscriberId.ToString()),
				MaxAllowedCopies = availableCopiesCount,
				CurrentCopies = _mapper.Map<IEnumerable<BookCopyViewModel>>(currentCopies),
				Key = rKey
			};
			return View("Form", viewModel);
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public IActionResult Edit(RentalFormViewModel model)
		{
			if (!ModelState.IsValid)
				return View("Form", model);

			model.Id = int.Parse(_dataProtector.Unprotect(model.Key!));
			var rental = _rentalService.GetDetails(model.Id ?? 0);

			if (rental is null || rental.CreatedOn.Date != DateTime.Today)
				return NotFound();

			var (errorMessage, maxAllowedCopies) = _subscriberService.CanRent(rental.SubscriberId, rental.Id);

			if (!string.IsNullOrEmpty(errorMessage))
				return View("NotAllowedRental", errorMessage);

			var (rentalsError, copies) = _bookCopyService.CanBeRented(model.SelectedCopies, rental.SubscriberId, rental.Id);

			if (!string.IsNullOrEmpty(rentalsError))
				return View("NotAllowedRental", rentalsError);

			_rentalService.Update(rental.Id, copies, User.GetUserId());

			return RedirectToAction(nameof(Details), new { rKey = model.Key });
		}

		public IActionResult Return(string rKey)
		{
			var rentalId = int.Parse(_dataProtector.Unprotect(rKey));

			var query = _rentalService.GetQueryableDetails(rentalId);

			var rental = _mapper.ProjectTo<RentalViewModel>(query).SingleOrDefault(r => r.Id == rentalId);
			if (rental is null || rental.CreatedOn.Date == DateTime.Now)
				return NotFound();

			var subscriber = _subscriberService.GetSubscriberWithSubscriptions(rental.Subscriber!.Id);

			//var rental = _rentalService.GetDetails(rentalId);

			//if (rental is null || rental.CreatedOn.Date == DateTime.Today)
			//	return NotFound();


			//RentalReturnFormViewModel viewModelL = new()
			//{
			//	Id = rentalId,
			//	Copies = _mapper.Map<IList<RentalCopyViewModel>>(rental.RentalCopies.Where(c => !c.ReturnDate.HasValue)).ToList(),
			//	SelectedCopies = rental.RentalCopies.Where(c => !c.ReturnDate.HasValue).Select(r => new ReturnCopyViewModel { Id = r.BookCopyId, IsReturned = r.ExtendedOn.HasValue ? false : null }).ToList(),
			//	AllowExtend = _rentalService.AllowExtend(rental, subscriberrr!)
			//};

			RentalReturnFormViewModel viewModel = new()
			{
				Id = rentalId,
				Copies = _mapper.Map<IList<RentalCopyViewModel>>(rental.RentalCopies.Where(c => !c.ReturnDate.HasValue)).ToList(),
				SelectedCopies = rental.RentalCopies.Where(c => !c.ReturnDate.HasValue).Select(r => new ReturnCopyViewModel { Id = r.BookCopy!.Id, IsReturned = r.ExtendedOn.HasValue ? false : null }).ToList(),
				AllowExtend = _rentalService.AllowExtend(rental.StartDate, subscriber!)
			};

			return View(viewModel);
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public IActionResult Return(RentalReturnFormViewModel model)
		{
			var rental = _rentalService.GetDetails(model.Id);

			if (rental is null || rental.CreatedOn.Date == DateTime.Today)
				return NotFound();

			var copies = _mapper.Map<IList<RentalCopyViewModel>>(rental.RentalCopies.Where(c => !c.ReturnDate.HasValue)).ToList();


			if (!ModelState.IsValid)
			{
				model.Copies = copies;
				return View(model);
			}

			var subscriber = _subscriberService.GetSubscriberWithSubscriptions(rental.SubscriberId);

			if (model.SelectedCopies.Any(c => c.IsReturned.HasValue && !c.IsReturned.Value))
			{
				var error = _rentalService.ValidateExtendedCopies(rental, subscriber!);

				if (!string.IsNullOrEmpty(error))
				{
					model.Copies = copies;
					ModelState.AddModelError("", error);
					return View(model);
				}
			}

			var copiesDto = _mapper.Map<IList<ReturnCopyDto>>(model.SelectedCopies);

			_rentalService.Return(rental, copiesDto, model.PenaltyPaid, User.GetUserId());

			var sKey = _dataProtector.Protect(model.Id.ToString());

			return RedirectToAction(nameof(Details), new { rKey = sKey });
		}
	}
}
