namespace BIMS.Web.Controllers
{

    [Authorize(Roles = AppRoles.Archive)]
    public class BookCopiesController : Controller
    {
        private readonly IDataProtector _dataProtector;
        private readonly IMapper _mapper;
        private readonly IBookService _bookService;
        private readonly IBookCopyService _bookCopyService;
        private readonly IRentalService _rentalService;


        public BookCopiesController(IDataProtectionProvider dataProtector, IMapper mapper, IBookCopyService bookCopyService, IBookService bookService, IRentalService rentalService)
        {
            _dataProtector = dataProtector.CreateProtector("SecureKey");
            _mapper = mapper;
            _bookCopyService = bookCopyService;
            _bookService = bookService;
            _rentalService = rentalService;
        }

        public IActionResult Index()
        {
            return View();
        }

        [AjaxOnly]
        public IActionResult Create(int BookId)
        {
            var book = _bookService.GetById(BookId);
            if (book == null)
                return NotFound();

            var bookCopy = new BookCopyViewModel()
            {
                BookId = BookId,
                ShowRentalInput = book.IsAvilableForRental
            };
            return PartialView("_Create", bookCopy);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CreatePartial(BookCopyViewModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest();

            var book = _bookService.GetById(model.BookId);
            if (book == null)
                return NotFound();


            var copy = _bookCopyService.Add(model.BookId, model.EditionNumber, model.IsAvailableForRental, User.GetUserId());

            if (copy is null)
                return NotFound();

            var viewModel = _mapper.Map<BookCopyViewModel>(copy);
            viewModel.bcKey = _dataProtector.Protect(model.Id.ToString());

            return PartialView("_BookCopyRow", viewModel);
        }

        [AjaxOnly]
        public IActionResult Edit(int Id)
        {
            var copy = _bookCopyService.GetDetails(Id);

            if (copy == null)
                return NotFound();

            var bookCopy = _mapper.Map<BookCopyViewModel>(copy);

            bookCopy.ShowRentalInput = copy.Book!.IsAvilableForRental;

            return PartialView("_Create", bookCopy);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EditPartial(BookCopyViewModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest();

            var copy = _bookCopyService.Update(model.Id, model.EditionNumber, model.IsAvailableForRental, User.GetUserId());
            var viewModel = _mapper.Map<BookCopyViewModel>(copy);
            viewModel.bcKey = _dataProtector.Protect(model.Id.ToString());

            return PartialView("_BookCopyRow", viewModel);
        }

        public IActionResult RentalHistory(string bcKey)
        {
            var bookCopyId = int.Parse(_dataProtector.Unprotect(bcKey));

            var copyHistory = _rentalService.GetAllByCopyId(bookCopyId);

            var viewModel = _mapper.Map<IEnumerable<CopyHistoryViewModel>>(copyHistory);

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ToggleStatus(int id)
        {
            var copy = _bookCopyService.ToggleStatus(id, User.GetUserId());

            return copy is null ? NotFound() : Ok();
        }
    }
}
