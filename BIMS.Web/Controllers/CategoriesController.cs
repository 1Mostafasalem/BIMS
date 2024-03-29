using Bookify.Web.Core.Models;
using System.Security.Claims;

namespace Bookify.Web.Controllers
{
    [Authorize(Roles = AppRoles.Archive)]
    public class CategoriesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;

        public CategoriesController(ApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        [HttpGet]
        public IActionResult Index()
        {
            //TODO : use ViewModel
            var categories = _context.Categories.AsNoTracking().ToList();
            var viewModel = _mapper.Map<IEnumerable<CategoryViewModel>>(categories);
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
        public IActionResult Create(CategoryViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);
            var category = _mapper.Map<Category>(model);
            category.CreatedById = User.FindFirst(ClaimTypes.NameIdentifier)!.Value;
            _context.Categories.Add(category);
            _context.SaveChanges();
            TempData["Message"] = "Saved Successfully";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CreatePartial(CategoryViewModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest();

            var category = _mapper.Map<Category>(model);
            category.CreatedById = User.FindFirst(ClaimTypes.NameIdentifier)!.Value;
            _context.Categories.Add(category);

            _context.SaveChanges();
            //TempData["Message"] = "Saved Successfully";
            var viewModel = _mapper.Map<CategoryViewModel>(category);
            return PartialView("_CategoryRow", viewModel);
        }

        [HttpGet]
        public IActionResult Edit(int id)
        {
            var category = _context.Categories.Find(id);
            if (category == null)
                return NotFound();

            var viewModel = _mapper.Map<CategoryViewModel>(category);
            return View(nameof(Create), viewModel);
        }

        [HttpGet]
        [AjaxOnlyAttribute]
        public IActionResult EditPartial(int Id)
        {
            var category = _context.Categories.Find(Id);
            if (category == null)
                return NotFound();

            var viewModel = _mapper.Map<CategoryViewModel>(category);
            return PartialView("_Create", viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(CategoryViewModel model)
        {
            if (!ModelState.IsValid)
                return View(nameof(Create), model);

            var category = _context.Categories.Find(model.Id);
            if (category == null)
                return NotFound();

            category = _mapper.Map(model, category);
            category.LastUpdatedOn = DateTime.Now;
            category.LastUpdatedById = User.FindFirst(ClaimTypes.NameIdentifier)!.Value;
            _context.SaveChanges();
            TempData["Message"] = "Saved Successfully";

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EditPartial(CategoryViewModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest();

            var category = _context.Categories.Find(model.Id);
            if (category == null)
                return NotFound();

            model.LastUpdatedOn = DateTime.Now;
            category = _mapper.Map(model, category);
            category.LastUpdatedById = User.FindFirst(ClaimTypes.NameIdentifier)!.Value;
            _context.SaveChanges();
            //TempData["Message"] = "Saved Successfully";
            return PartialView("_CategoryRow", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ToggleStatus(int id)
        {
            var category = _context.Categories.Find(id);
            if (category == null)
                return NotFound();

            category.IsDeleted = !category.IsDeleted;
            category.LastUpdatedOn = DateTime.Now;
            _context.SaveChanges();
            return Ok(category.LastUpdatedOn.ToString());
        }

        public ActionResult AllowItem(CategoryViewModel model)
        {
            var categories = _context.Categories.Where(c => c.Name == model.Name);
            bool isExist;
            if (model.Id == 0)
                isExist = categories.Any();
            else
                isExist = categories.Any(x => x.Id != model.Id);

            return Json(!isExist);
        }
    }
}
