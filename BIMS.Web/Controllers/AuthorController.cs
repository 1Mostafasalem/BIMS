using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BIMS.Web.Controllers
{
    [Authorize(Roles = AppRoles.Archive)]
    public class AuthorController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;

        public AuthorController(ApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        [HttpGet]
        public IActionResult Index()
        {
            //TODO : use ViewModel
            var authors = _context.Authors.AsNoTracking().ToList();
            var viewModel = _mapper.Map<IEnumerable<AuthorViewModel>>(authors);
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
            var author = _mapper.Map<Author>(model);
            author.CreatedById = User.FindFirst(ClaimTypes.NameIdentifier)!.Value;
            _context.Authors.Add(author);
            _context.SaveChanges();
            TempData["Message"] = "Saved Successfully";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CreatePartial(AuthorViewModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest();

            var author = _mapper.Map<Author>(model);
            author.CreatedById = User.FindFirst(ClaimTypes.NameIdentifier)!.Value;
            _context.Authors.Add(author);
            _context.SaveChanges();
            var viewModel = _mapper.Map<AuthorViewModel>(author);
            return PartialView("_CategoryRow", viewModel);
        }

        [HttpGet]
        public IActionResult Edit(int id)
        {
            var author = _context.Authors.Find(id);
            if (author == null)
                return NotFound();

            var viewModel = _mapper.Map<AuthorViewModel>(author);
            return View(nameof(Create), viewModel);
        }

        [HttpGet]
        [AjaxOnlyAttribute]
        public IActionResult EditPartial(int Id)
        {
            var author = _context.Authors.Find(Id);
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

            var author = _context.Authors.Find(model.Id);
            if (author == null)
                return NotFound();
            model.LastUpdatedOn = DateTime.Now;
            author = _mapper.Map(model, author);
            author.LastUpdatedById = User.FindFirst(ClaimTypes.NameIdentifier)!.Value;

            _context.SaveChanges();
            TempData["Message"] = "Saved Successfully";

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EditPartial(AuthorViewModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest();

            var author = _context.Authors.Find(model.Id);
            if (author == null)
                return NotFound();
            model.LastUpdatedOn = DateTime.Now;
            author = _mapper.Map(model, author);
            author.LastUpdatedById = User.FindFirst(ClaimTypes.NameIdentifier)!.Value;
            _context.SaveChanges();
            //TempData["Message"] = "Saved Successfully";
            return PartialView("_AuthorRow", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ToggleStatus(int id)
        {
            var author = _context.Authors.Find(id);
            if (author == null)
                return NotFound();

            author.IsDeleted = !author.IsDeleted;
            author.LastUpdatedOn = DateTime.Now;
            author.LastUpdatedById = User.FindFirst(ClaimTypes.NameIdentifier)!.Value;
            _context.SaveChanges();
            return Ok(author.LastUpdatedOn.ToString());
        }

        public ActionResult AllowItem(CategoryViewModel model)
        {
            var authors = _context.Authors.Where(c => c.Name == model.Name);
            bool isExist;
            if (model.Id == 0)
                isExist = authors.Any();
            else
                isExist = authors.Any(x => x.Id != model.Id);

            return Json(!isExist);
        }
    }
}
