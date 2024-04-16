using BIMS.Application.Common.Interfaces.Repositories;

namespace BIMS.Web.Controllers
{
    [Authorize(Roles = AppRoles.Archive)]
    public class CategoriesController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public CategoriesController(IMapper mapper, IUnitOfWork unitOfWork)
        {
            _mapper = mapper;
            _unitOfWork = unitOfWork;
        }

        [HttpGet]
        public IActionResult Index()
        {
            //TODO : use ViewModel
            var categories = _unitOfWork.Categories.GetAll();
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
            category.CreatedById = User.GetUserId();

            _unitOfWork.Categories.Add(category);
            _unitOfWork.Commit();

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

            category.CreatedById = User.GetUserId();

            _unitOfWork.Categories.Add(category);
            _unitOfWork.Commit();

            //TempData["Message"] = "Saved Successfully";
            var viewModel = _mapper.Map<CategoryViewModel>(category);
            return PartialView("_CategoryRow", viewModel);
        }

        [HttpGet]
        public IActionResult Edit(int id)
        {
            var category = _unitOfWork.Categories.GetById(id);
            if (category == null)
                return NotFound();

            var viewModel = _mapper.Map<CategoryViewModel>(category);
            return View(nameof(Create), viewModel);
        }

        [HttpGet]
        [AjaxOnlyAttribute]
        public IActionResult EditPartial(int Id)
        {
            var category = _unitOfWork.Categories.GetById(Id);
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

            var category = _unitOfWork.Categories.GetById(model.Id);
            if (category == null)
                return NotFound();

            category = _mapper.Map(model, category);
            category.LastUpdatedOn = DateTime.Now;
            category.LastUpdatedById = User.FindFirst(ClaimTypes.NameIdentifier)!.Value;
            _unitOfWork.Commit();
            TempData["Message"] = "Saved Successfully";

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EditPartial(CategoryViewModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest();

            var category = _unitOfWork.Categories.GetById(model.Id);
            if (category == null)
                return NotFound();

            model.LastUpdatedOn = DateTime.Now;
            category = _mapper.Map(model, category);
            category.LastUpdatedById = User.FindFirst(ClaimTypes.NameIdentifier)!.Value;
            _unitOfWork.Commit();
            //TempData["Message"] = "Saved Successfully";
            return PartialView("_CategoryRow", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ToggleStatus(int id)
        {
            var category = _unitOfWork.Categories.GetById(id);
            if (category == null)
                return NotFound();

            category.IsDeleted = !category.IsDeleted;
            category.LastUpdatedOn = DateTime.Now;
            _unitOfWork.Commit();
            return Ok(category.LastUpdatedOn.ToString());
        }

        public ActionResult AllowItem(CategoryViewModel model)
        {
            var categories = _unitOfWork.Categories.Find(c => c.Name == model.Name);
            var isAllowed = (categories is null || categories.Id == model.Id);

            return Json(isAllowed);
        }
    }
}
