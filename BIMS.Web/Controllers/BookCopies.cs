using Microsoft.AspNetCore.Mvc;

namespace BIMS.Web.Controllers
{

    [Authorize(Roles = AppRoles.Archive)]
    public class BookCopies : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;

        public BookCopies(ApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public IActionResult Index()
        {
            return View();
        }

        [AjaxOnly]
        public IActionResult Create(int BookId)
        {
            var book = _context.Books.Find(BookId);
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

            var book = _context.Books.Find(model.BookId);
            if (book == null)
                return NotFound();

            var copy = new BookCopy
            {
                BookId = model.BookId,
                EditionNumber = model.EditionNumber,
                IsAvailableForRental = book.IsAvilableForRental && model.IsAvailableForRental,
                CreatedById = User.FindFirst(ClaimTypes.NameIdentifier)!.Value
            };

            _context.BookCopies.Add(copy);
            _context.SaveChanges();

            var bookCopy = _mapper.Map<BookCopyViewModel>(copy);

            return PartialView("_BookCopyRow",bookCopy);
        }

        [AjaxOnly]
        public IActionResult Edit(int Id)
        {
            var copy = _context.BookCopies.Find(Id);
            if (copy == null)
                return NotFound();

            var book = _context.Books.Find(copy.BookId);
            if (book == null)
                return NotFound();

            var bookCopy = _mapper.Map<BookCopyViewModel>(copy);

            bookCopy.ShowRentalInput = book.IsAvilableForRental;


            return PartialView("_Create", bookCopy);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EditPartial(BookCopyViewModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest();

            var book = _context.Books.Find(model.BookId);
            if (book == null)
                return NotFound();

            var copy = _context.BookCopies.Find(model.Id);
            if (copy == null)
                return BadRequest();

            copy.EditionNumber = model.EditionNumber;
            copy.IsAvailableForRental = model.IsAvailableForRental;
            copy.LastUpdatedOn = DateTime.Now;
            copy.LastUpdatedById = User.FindFirst(ClaimTypes.NameIdentifier)!.Value;


            _context.BookCopies.Update(copy);
            _context.SaveChanges();

            var bookCopy = _mapper.Map<BookCopyViewModel>(copy);

            return PartialView("_BookCopyRow", bookCopy);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ToggleStatus(int id)
        {
            var book = _context.BookCopies.Find(id);
            if (book is null)
                return NotFound();

            book.IsDeleted = !book.IsDeleted;
            book.LastUpdatedOn = DateTime.Now;
            book.LastUpdatedById = User.FindFirst(ClaimTypes.NameIdentifier)!.Value;
            _context.SaveChanges();
            return Ok();
        }
    }
}
