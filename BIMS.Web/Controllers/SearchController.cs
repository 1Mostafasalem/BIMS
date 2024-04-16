using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace BIMS.Web.Controllers
{
	public class SearchController : Controller
	{
        private readonly IBookService _bookService;
        private readonly IDataProtector _dataProtector;
		private readonly IMapper _mapper;


        public SearchController(IMapper mapper, IDataProtectionProvider dataProtector, IBookService bookService)
        {
            _mapper = mapper;
            _dataProtector = dataProtector.CreateProtector("SecureKey");
            _bookService = bookService;
        }


        public IActionResult Index()
		{
			return View();
		}

		public IActionResult Find(string query)
		{
            var books = _bookService.Search(query);

            var data = _mapper.ProjectTo<BookSearchResultViewModel>(books).ToList();

			foreach (var book in data)
				book.Key = _dataProtector.Protect(book.Id.ToString());

            return Ok(data);
		}

		public IActionResult Details(string bKey)
		{
			var bookId = int.Parse(_dataProtector.Unprotect(bKey));

            var query = _bookService.GetDetails();

            var viewModel = _mapper.ProjectTo<BookViewModel>(query)
                .SingleOrDefault(b => b.Id == bookId);

            if (viewModel is null)
				return NotFound();


			return View(viewModel);
		}
	}
}
