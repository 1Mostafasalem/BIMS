namespace BIMS.Web.Controllers
{
	[Authorize(Roles = AppRoles.Archive)]
	public class AuthorController : Controller
	{
		private readonly IMapper _mapper;
		private readonly IDataProtector _dataProtector;
		private readonly IAuthorService _authorService;

		public AuthorController(IMapper mapper, IDataProtectionProvider dataProtector, IAuthorService authorService)
		{
			_mapper = mapper;
			_dataProtector = dataProtector.CreateProtector("SecureKey");
			_authorService = authorService;
		}

		[HttpGet]
		public IActionResult Index()
		{
			var authors = _authorService.GetAll();
			var viewModel = _mapper.Map<IEnumerable<AuthorViewModel>>(authors);
			foreach (var author in viewModel)
				author.Key = _dataProtector.Protect(author.Id.ToString());

			return View(viewModel);
		}

		[HttpGet]
		public IActionResult Create()
		{
			return View();
		}

		[HttpGet]
		[AjaxOnlyAttribute]
		public IActionResult CreatePartial()
		{
			return PartialView("_Create");
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public IActionResult Create(AuthorViewModel model)
		{
			if (!ModelState.IsValid)
				return View(model);

			var author = _authorService.Add(model.Name, User.GetUserId());


			TempData["Message"] = "Saved Successfully";
			return RedirectToAction(nameof(Index));
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public IActionResult CreatePartial(AuthorViewModel model)
		{
			if (!ModelState.IsValid)
				return BadRequest();

			var author = _authorService.Add(model.Name, User.GetUserId());

			var viewModel = _mapper.Map<AuthorViewModel>(author);
			viewModel.Key = _dataProtector.Protect(author.Id.ToString());

			return PartialView("_AuthorRow", viewModel);
		}

		[HttpGet]
		public IActionResult Edit(string key)
		{
			var authorId = int.Parse(_dataProtector.Unprotect(key));

			var author = _authorService.GetById(authorId);
			if (author == null)
				return NotFound();

			var viewModel = _mapper.Map<AuthorViewModel>(author);
			return View(nameof(Create), viewModel);
		}

		[HttpGet]
		[AjaxOnlyAttribute]
		public IActionResult EditPartial(string key)
		{
			var authorId = int.Parse(_dataProtector.Unprotect(key));

			var author = _authorService.GetById(authorId);
			if (author == null)
				return NotFound();

			var viewModel = _mapper.Map<AuthorViewModel>(author);
			return PartialView("_Create", viewModel);
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public IActionResult Edit(AuthorViewModel model)
		{
			if (!ModelState.IsValid)
				return View(nameof(Create), model);

			var authorId = int.Parse(_dataProtector.Unprotect(model.Key!));

			var author = _authorService.Update(authorId, model.Name, User.GetUserId());
			if (author is null)
				return NotFound();

			TempData["Message"] = "Saved Successfully";

			return RedirectToAction(nameof(Index));
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public IActionResult EditPartial(AuthorViewModel model)
		{
			if (!ModelState.IsValid)
				return BadRequest();

			var authorId = int.Parse(_dataProtector.Unprotect(model.Key!));

			var author = _authorService.Update(authorId, model.Name, User.GetUserId());

			if (author is null)
				return NotFound();

			//TempData["Message"] = "Saved Successfully";
			return PartialView("_AuthorRow", _mapper.Map<AuthorViewModel>(author));
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public IActionResult ToggleStatus(string key)
		{
			var authorId = int.Parse(_dataProtector.Unprotect(key));

			var author = _authorService.ToggleStatus(authorId, User.GetUserId());
			if (author is null)
				return NotFound();

			return Ok(author.LastUpdatedOn.ToString());
		}

		public ActionResult AllowItem(AuthorViewModel model)
		{
			var isAllowed = _authorService.AllowAuthor(model.Id, model.Name);

			return Json(isAllowed);
		}
	}
}
