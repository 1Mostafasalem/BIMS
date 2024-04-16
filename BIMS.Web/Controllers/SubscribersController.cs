namespace BIMS.Web.Controllers
{
	[Authorize(Roles = AppRoles.Reception)]
	public class SubscribersController : Controller
	{
		private readonly IDataProtector _dataProtector;
		private readonly IMapper _mapper;
		private readonly IImageService _imageService;
		private readonly ISubscriberService _subscriberService;
		private readonly IGovernoratesService _governorateService;
		private readonly IAreaService _areaService;

		public SubscribersController(IDataProtectionProvider dataProtector, IMapper mapper, IImageService imageService, ISubscriberService subscriberService, IAreaService areaService, IGovernoratesService governorateService)
		{
			_dataProtector = dataProtector.CreateProtector("SecureKey");
			_mapper = mapper;
			_imageService = imageService;
			_subscriberService = subscriberService;
			_areaService = areaService;
			_governorateService = governorateService;
		}

		public IActionResult Index()
		{
			return View();
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public IActionResult Search(SubscriberSearchViewModel model)
		{
			if (!ModelState.IsValid)
				return BadRequest(ModelState);

			var query = _subscriberService.GetQueryable();

			var subscriber = _mapper.ProjectTo<SubscriberSearchResultViewModel>(query)
									.SingleOrDefault(s =>
													   s.Email == model.Value
													|| s.NationalId == model.Value
													|| s.MobileNumber == model.Value);

			if (subscriber is not null)
				subscriber.Key = _dataProtector.Protect(subscriber.Id.ToString());

			return PartialView("_Result", subscriber);
		}

		public IActionResult Create()
		{
			SubscriberViewModel model = PopulateViewModel();
			return View(nameof(Create), model);
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Create(SubscriberViewModel model)
		{
			if (!ModelState.IsValid)
				return View("Create", PopulateViewModel(model));

			Subscriber subscriber = _mapper.Map<Subscriber>(model);

			var imageName = $"{Guid.NewGuid()}{Path.GetExtension(model.Image!.FileName)}";
			var imagePath = "/images/Subscribers";

			var (isUploaded, errorMessage) = await _imageService.UploadAsync(model.Image, imageName, imagePath, hasThumbnail: true);

			if (!isUploaded)
			{
				ModelState.AddModelError("Image", errorMessage!);
				return View("Create", PopulateViewModel(model));
			}

			subscriber = _subscriberService.Add(subscriber, imagePath, imageName, User.GetUserId());

			var subscriberId = _dataProtector.Protect(subscriber.Id.ToString());
			return RedirectToAction(nameof(Details), new { id = subscriberId });

		}

		public IActionResult Details(string id)
		{
			var subscriberId = int.Parse(_dataProtector.Unprotect(id));

			var query = _subscriberService.GetQueryableDetails();

			var viewModel = _mapper.ProjectTo<SubscriberDetailViewModel>(query).SingleOrDefault(s => s.Id == subscriberId);

			if (viewModel is null)
				return NotFound();


			viewModel.Key = _dataProtector.Protect(subscriberId.ToString());
			var tKey = id;

			foreach (var rental in viewModel.Rentals)
				rental.Key = _dataProtector.Protect(rental.Id.ToString());

			return View(viewModel);
		}

		public IActionResult Edit(string skey)
		{
			var subscriberId = int.Parse(_dataProtector.Unprotect(skey));

			var subscriber = _subscriberService.GetById(subscriberId);

			if (subscriber is null)
				return NotFound();


			SubscriberViewModel model = _mapper.Map<SubscriberViewModel>(subscriber);
			var viewModel = PopulateViewModel(model);
			viewModel.Key = skey;
			return View("Create", viewModel);

		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Edit(SubscriberViewModel model)
		{
			if (!ModelState.IsValid)
				return View("Create", PopulateViewModel(model));

			var subscriberId = int.Parse(_dataProtector.Unprotect(model.Key!));

			var subscriber = _subscriberService.GetById(subscriberId);
			model.Id = subscriberId;

			if (subscriber is null)
				return NotFound();

			if (model.Image is not null)
			{
				if (!string.IsNullOrEmpty(subscriber.ImageUrl))
					_imageService.Delete(subscriber.ImageUrl, subscriber.ImageThumbnailUrl);

				var imageName = $"{Guid.NewGuid()}{Path.GetExtension(model.Image.FileName)}";
				var imagePath = "/images/Subscribers";

				var (isUploaded, errorMessage) = await _imageService.UploadAsync(model.Image, imageName, imagePath, hasThumbnail: true);

				if (!isUploaded)
				{
					ModelState.AddModelError("Image", errorMessage!);
					return View("Create", PopulateViewModel(model));
				}

				model.ImageUrl = $"{imagePath}/{imageName}";
				model.ImageThumbnailUrl = $"{imagePath}/thumb/{imageName}";
			}
			else if (!string.IsNullOrEmpty(subscriber.ImageUrl))
			{
				model.ImageUrl = subscriber.ImageUrl;
				model.ImageThumbnailUrl = subscriber.ImageThumbnailUrl;
			}

			subscriber = _mapper.Map(model, subscriber);

			_subscriberService.Update(subscriber, User.GetUserId());

			//subscriber.LastUpdatedById = User.FindFirst(ClaimTypes.NameIdentifier)!.Value;
			//subscriber.LastUpdatedOn = DateTime.Now;

			//_context.Subscribers.Update(subscriber);
			//_context.SaveChanges();

			var skey = _dataProtector.Protect(subscriber.Id.ToString());
			return RedirectToAction(nameof(Details), new { id = skey });
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public IActionResult RenewSubscription(string skey)
		{
			var subscriberId = int.Parse(_dataProtector.Unprotect(skey));

			var subscriber = _subscriberService.GetSubscriberWithSubscriptions(subscriberId);

			if (subscriber is null)
				return NotFound();
			if (subscriber.IsBlackListed)
				return BadRequest();

			var lastSubscription = subscriber.Subscriptions.Last();

			var startDate = lastSubscription.EndDate < DateTime.Today ? DateTime.Today : lastSubscription.EndDate.AddDays(1);

			var subscription = _subscriberService.RenewSubscription(subscriberId, startDate, User.GetUserId());

			//TODO: Send email

			var viewModel = _mapper.Map<SubscriptionViewModel>(subscription);
			return PartialView("_SubscriptionRow", viewModel);
		}
		public IActionResult AllowNationalId(SubscriberViewModel model)
		{
			var subscribersId = 0;
			if (!string.IsNullOrEmpty(model.Key))
				subscribersId = int.Parse(_dataProtector.Unprotect(model.Key));

			return Json(_subscriberService.AllowNationalId(subscribersId, model.NationalId)); ;
		}

		public IActionResult AllowMobileNumber(SubscriberViewModel model)
		{
			var subscribersId = 0;
			if (!string.IsNullOrEmpty(model.Key))
				subscribersId = int.Parse(_dataProtector.Unprotect(model.Key));

			return Json(_subscriberService.AllowMobileNumber(subscribersId, model.MobileNumber)); ;
		}

		public IActionResult AllowEmail(SubscriberViewModel model)
		{
			var subscribersId = 0;
			if (!string.IsNullOrEmpty(model.Key))
				subscribersId = int.Parse(_dataProtector.Unprotect(model.Key));

			return Json(_subscriberService.AllowEmail(subscribersId, model.Email)); ;
		}

		[AjaxOnly]
		public IActionResult GetAreas(int governorateId)
		{
			var areas = _areaService.GetActiveAreasByGovernorateId(governorateId);

			return Ok(_mapper.Map<IEnumerable<SelectListItem>>(areas));
		}

		private SubscriberViewModel PopulateViewModel(SubscriberViewModel? model = null)
		{
			SubscriberViewModel viewModel = model is null ? new SubscriberViewModel() : model;

			var governorates = _governorateService.GetActiveGovernorates();
			viewModel.Governorates = _mapper.Map<IEnumerable<SelectListItem>>(governorates);

			if (model?.GovernorateId > 0)
			{
				var areas = _areaService.GetActiveAreasByGovernorateId(model.GovernorateId);
				viewModel.Areas = _mapper.Map<IEnumerable<SelectListItem>>(areas);
			}

			return viewModel;
		}

		[HttpPost]
		public IActionResult ToggleBlackListStatus(string skey)
		{
			var subscriberId = int.Parse(_dataProtector.Unprotect(skey));
			_subscriberService.ToggleBlackListStatus(subscriberId, User.GetUserId());

			return RedirectToAction(nameof(Details), new { id = skey });
		}

	}
}
