namespace BIMS.Web.Controllers
{
	[Authorize(Roles = AppRoles.Archive)]
	public class BooksController : Controller
    {
        private readonly IDataProtector _dataProtector;
        private readonly IMapper _mapper;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IImageService _imageService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IBookService _bookService;
        private readonly IAuthorService _authorService;
        private readonly ICategoryService _categoryService;

        public BooksController(IMapper mapper
            , IWebHostEnvironment webHostEnvironment, IImageService imageService, IDataProtectionProvider dataProtector, IUnitOfWork unitOfWork, IBookService bookService, IAuthorService authorService, ICategoryService categoryService)
        {
            _mapper = mapper;
            _webHostEnvironment = webHostEnvironment;
            _imageService = imageService;
            _dataProtector = dataProtector.CreateProtector("SecureKey");
            _unitOfWork = unitOfWork;
            _bookService = bookService;
            _authorService = authorService;
            _categoryService = categoryService;
        }

        public IActionResult Index()
        {
            return View();
        }

        [AjaxOnly]
        [HttpPost, IgnoreAntiforgeryToken]
        public IActionResult GetBooks()
        {
            var filterDto = Request.Form.GetFilters();

            var (books, recordsTotal) = _bookService.GetFiltered(filterDto);

            var mappedData = _mapper.ProjectTo<BookRowViewModel>(books).ToList();

            foreach (var book in mappedData)
                book.bKey = _dataProtector.Protect(book.Id.ToString());

            return Ok(new { recordsFiltered = recordsTotal, recordsTotal, data = mappedData });
        }
        public IActionResult Details(string bkey)
        {
            var bookId = int.Parse(_dataProtector.Unprotect(bkey));


            var query = _bookService.GetDetails();

            var model = _mapper.ProjectTo<BookViewModel>(query)
                .FirstOrDefault(x => x.Id == bookId);

            if (model is null)
                return NotFound();

            model.bKey = _dataProtector.Protect(model.Id.ToString());
            model.Copies.ToList().ForEach(c => c.bcKey = _dataProtector.Protect(c.Id.ToString()));

            return View(model);
        }

        public IActionResult Create()
        {
            var viewModel = PopulateViewModel();
            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(BookViewModel model)
        {
            if (!ModelState.IsValid)
                return View(PopulateViewModel(model));

            var book = _mapper.Map<Book>(model);

            if (model.Image is not null)
            {
                var imageName = $"{Guid.NewGuid()}{Path.GetExtension(model.Image.FileName)}";

                var (isUploaded, errorMessage) = await _imageService.UploadAsync(model.Image, imageName, "/images/books", hasThumbnail: true);

                if (isUploaded)
                {
                    book.ImageUrl = $"/images/books/{imageName}";
                    book.ImageThumbnailUrl = $"/images/books/thumb/{imageName}";
                }
                else
                {
                    ModelState.AddModelError("", errorMessage!);
                    return View("Form", PopulateViewModel(model));
                }
            }

            _bookService.Add(book, User.GetUserId());

            var bKey = _dataProtector.Protect(book.Id.ToString());

            return RedirectToAction(nameof(Details), new { bkey = bKey });
        }

        public IActionResult Edit(string bkey)
        {
            var bookId = int.Parse(_dataProtector.Unprotect(bkey));

            var book = _bookService.GetWithCategories(bookId);

            if (book is null)
                return NotFound();

            var model = _mapper.Map<BookViewModel>(book);
            var viewModel = PopulateViewModel(model);

            viewModel.SelectedCategories = book.Categories.Select(x => x.CategoryId).ToList();

            return View(nameof(Create), viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(BookViewModel model)
        {
            if (!ModelState.IsValid)
                return View(PopulateViewModel(model));

            var book = _bookService.GetWithCategories(model.Id);

            if (book is null)
                return NotFound();


            if (model.Image is not null)
            {
                //Delete Old Image 
                if (!string.IsNullOrEmpty(book.ImageUrl))
                {
                    _imageService.Delete(book.ImageUrl, book.ImageThumbnailUrl);
                }

                var imageName = $"{Guid.NewGuid()}{Path.GetExtension(model.Image.FileName)}";

                var (isUploaded, errorMessage) = await _imageService.UploadAsync(model.Image, imageName, "/images/books", true);

                if (isUploaded)
                {
                    model.ImageUrl = $"/images/books/{imageName}";
                    model.ImageThumbnailUrl = $"/images/books/thumb/{imageName}";
                }
                else
                {
                    ModelState.AddModelError("", errorMessage!);
                    return View("Form", PopulateViewModel(model));
                }
            }
            else if (!string.IsNullOrEmpty(book.ImageUrl))
            {
                model.ImageUrl = book.ImageUrl;
                model.ImageThumbnailUrl = book.ImageThumbnailUrl;
            }

            book = _mapper.Map(model, book);
            book = _bookService.Update(book, User.GetUserId());

            var bKey = _dataProtector.Protect(book.Id.ToString());

            return RedirectToAction(nameof(Details), new { bkey = bKey });
        }
        private BookViewModel PopulateViewModel(BookViewModel? model = null)
        {
            BookViewModel viewModel = model is null ? new BookViewModel() : model;
            var authors = _authorService.GetActiveAuthors();
            var categories = _categoryService.GetActiveCategories();

            viewModel.Authors = _mapper.Map<IEnumerable<SelectListItem>>(authors);
            viewModel.Categories = _mapper.Map<IEnumerable<SelectListItem>>(categories);
            return viewModel;
        }
        public ActionResult AllowItem(BookViewModel model)
        {
            return Json(_bookService.AllowTitle(model.Id, model.Title!, model.AuthorId));
        }

        private string GetThumbinalUrl(string url)
        {
            var separator = "image/upload/";
            var urlParts = url.Split(separator);
            var thumbinalUrl = $"{urlParts[0]}{separator}c_thumb,w_200,g_face/{urlParts[1]}";
            return thumbinalUrl;
        }

        [ValidateAntiForgeryToken]
        public IActionResult ToggleStatus(string bkey)
        {
            var bookId = int.Parse(_dataProtector.Unprotect(bkey));

            _bookService.ToggleStatus(bookId, User.GetUserId());

            return Ok();
        }
    }
}
