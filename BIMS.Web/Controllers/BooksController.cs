using Bookify.Web.Core.Models;
using Bookify.Web.Settings;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.Extensions.Options;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using System.IO;
using System.Linq;
using System.Linq.Dynamic.Core;

namespace Bookify.Web.Controllers
{
	public class BooksController : Controller
	{
		private readonly ApplicationDbContext _context;
		private readonly IMapper _mapper;
		private readonly IWebHostEnvironment _webHostEnvironment;
		private readonly Cloudinary _cloudinary;
		private readonly IImageService _imageService;

		private List<string> _allowedExtensions = new() { ".jpg", ".jpeg", ".png" };
		private int _maxAllowedSize = 2097152; //2MB

		public BooksController(ApplicationDbContext context, IMapper mapper
			, IWebHostEnvironment webHostEnvironment, IOptions<CloudinarySetting> cloudinary, IImageService imageService)
		{
			_context = context;
			_mapper = mapper;
			_webHostEnvironment = webHostEnvironment;
			Account account = new()
			{
				ApiKey = cloudinary.Value.APIKey,
				ApiSecret = cloudinary.Value.APISecret,
				Cloud = cloudinary.Value.CloudName,
			};
			_cloudinary = new Cloudinary(account);
			_imageService = imageService;
		}

		public IActionResult Index()
		{
			return View();
		}
		[AjaxOnly]
		[HttpPost]
		public IActionResult GetBooks()
		{
			var skip = int.Parse(Request.Form["start"]);
			var pageSize = int.Parse(Request.Form["length"]);
			var sortColumnIndex = Request.Form["order[0][column]"];
			var sortColumn = Request.Form[$"columns[{sortColumnIndex}][name]"];
			var sortColumnDirection = Request.Form["order[0][dir]"];
			var searchValue = Request.Form["search[value]"];


			IQueryable<Book> books = _context.Books.
				 Include(a => a.Author)
				.Include(b => b.Categories)
				.ThenInclude(c => c.Category)
				.Where(x => string.IsNullOrEmpty(searchValue) || x.Title.Contains(searchValue) || x.Author!.Name.Contains(searchValue));

			books = books.OrderBy($"{sortColumn} {sortColumnDirection}");
			var data = books.Skip(skip).Take(pageSize).ToList();

			var mappedData = _mapper.Map<IEnumerable<BookViewModel>>(data);

			var recordsTotal = books.Count();

			return Ok(new { recordsFiltered = recordsTotal, recordsTotal, data = mappedData });
		}
		public IActionResult Details(int id)
		{
			var book = _context.Books
				.Include(a => a.Author)
				.Include(b => b.Copies)
				.Include(b => b.Categories)
				.ThenInclude(c => c.Category)
				.FirstOrDefault(x => x.Id == id);

			if (book is null)
				return NotFound();

			//var model = _mapper.Map<BookDetailViewModel>(book);
			var model = _mapper.Map<BookViewModel>(book);

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

			//foreach (var category in model.SelectedCategories)
			//    book.Categories.Add(new BookCategory { CategoryId = category });
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

				//var extenstion = Path.GetExtension(model.Image.FileName);
				//if (!_allowedExtensions.Contains(extenstion))
				//{
				//    ModelState.AddModelError(nameof(model.Image), Errors.NotAllowedExtensions);
				//    return View(PopulateViewModel(model));
				//}

				//if (model.Image.Length > _maxAllowedSize)
				//{
				//    ModelState.AddModelError(nameof(model.Image), Errors.MaxSize);
				//    return View(PopulateViewModel(model));
				//}
				//var imageName = $"{Guid.NewGuid()}{extenstion}";

				//var path = Path.Combine($"{_webHostEnvironment.WebRootPath}/images/books", imageName);
				//var thumbPath = Path.Combine($"{_webHostEnvironment.WebRootPath}/images/books/thumb", imageName);
				//using var stream = System.IO.File.Create(path);
				//await model.Image.CopyToAsync(stream);
				//stream.Dispose();

				//book.ImageUrl = $"/images/books/{imageName}";
				//book.ImageThumbnailUrl = $"/images/books/thumb/{imageName}";

				//using var image = Image.Load(model.Image.OpenReadStream());
				//var ratio = (float)image.Width / 200;
				//var height = image.Height / ratio;
				//image.Mutate(i => i.Resize(width: 200, height: (int)height));
				//image.Save(thumbPath);

				//using var stream = model.Image.OpenReadStream();
				//var imagdeParams = new ImageUploadParams
				//{
				//    File = new FileDescription(imageName, stream),
				//    UseFilename = true
				//};

				//var result = await _cloudinary.UploadAsync(imagdeParams);
				//book.ImageUrl = result.SecureUrl.ToString();
				//book.ImageThumbnailUrl = GetThumbinalUrl(result.SecureUrl.ToString());
				//book.ImagePublicId = result.PublicId;
			}

			//model.SelectedCategories.ToList().ForEach(x => book.Categories.Add(new BookCategory { CategoryId = x }));

			book.CreatedById = User.FindFirst(ClaimTypes.NameIdentifier)!.Value;

			_context.Add(book);
			_context.SaveChanges();

			return RedirectToAction(nameof(Details), new { id = book.Id });
		}

		public IActionResult Edit(int id)
		{
			var book = _context.Books.Include(b => b.Categories).ThenInclude(c => c.Category).FirstOrDefault(x => x.Id == id);
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

			var book = _context.Books.Include(b => b.Categories)
				.Include(c => c.Copies)
				.FirstOrDefault(x => x.Id == model.Id);

			if (book is null)
				return NotFound();


			if (model.Image is not null)
			{
				//Delete Old Image 
				if (!string.IsNullOrEmpty(book.ImageUrl))
				{
					_imageService.Delete(book.ImageUrl, book.ImageThumbnailUrl);

					//var oldImagePath = $"{_webHostEnvironment.WebRootPath}{book.ImageUrl}";
					//var oldThumbPath = $"{_webHostEnvironment.WebRootPath}{book.ImageThumbnailUrl}";

					//if (System.IO.File.Exists(oldImagePath))
					//	System.IO.File.Delete(oldImagePath);

					//if (System.IO.File.Exists(oldThumbPath))
					//	System.IO.File.Delete(oldThumbPath);

					//await _cloudinary.DeleteResourcesAsync(book.ImagePublicId);
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




				//using var stream = model.Image.OpenReadStream();
				//var imagdeParams = new ImageUploadParams
				//{
				//    File = new FileDescription(imageName, stream),
				//    UseFilename = true
				//};

				//var result = await _cloudinary.UploadAsync(imagdeParams);
				//model.ImageUrl = result.SecureUrl.ToString();
				//model.ImageThumbnailUrl = GetThumbinalUrl(result.SecureUrl.ToString());
				//model.ImagePublicId = result.PublicId;

			}
			else if (!string.IsNullOrEmpty(book.ImageUrl))
			{
				model.ImageUrl = book.ImageUrl;
				model.ImageThumbnailUrl = book.ImageThumbnailUrl;
			}
			model.Copies = _mapper.Map<IEnumerable<BookCopyViewModel>>(book.Copies);
			book = _mapper.Map(model, book);
			book.LastUpdatedOn = DateTime.Now;
			book.LastUpdatedById = User.FindFirst(ClaimTypes.NameIdentifier)!.Value;

			if (!model.IsAvilableForRental)
				foreach (var copy in book.Copies)
					copy.IsAvailableForRental = false;

			_context.SaveChanges();

			return RedirectToAction(nameof(Details), new { id = book.Id });
		}
		private BookViewModel PopulateViewModel(BookViewModel? model = null)
		{
			BookViewModel viewModel = model is null ? new BookViewModel() : model;
			var authors = _context.Authors.Where(x => !x.IsDeleted).OrderBy(a => a.Name).ToList();
			var categories = _context.Categories.Where(x => !x.IsDeleted).OrderBy(a => a.Name).ToList();

			viewModel.Authors = _mapper.Map<IEnumerable<SelectListItem>>(authors);
			viewModel.Categories = _mapper.Map<IEnumerable<SelectListItem>>(categories);
			return viewModel;
		}
		public ActionResult AllowItem(BookViewModel model)
		{
			var authors = _context.Books.Where(b => b.Title == model.Title && b.AuthorId == model.AuthorId);
			bool isExist;
			if (model.Id == 0)
				isExist = authors.Any();
			else
				isExist = authors.Any(x => x.Id != model.Id);

			return Json(!isExist);
		}

		private string GetThumbinalUrl(string url)
		{
			var separator = "image/upload/";
			var urlParts = url.Split(separator);
			var thumbinalUrl = $"{urlParts[0]}{separator}c_thumb,w_200,g_face/{urlParts[1]}";
			return thumbinalUrl;
		}

		[ValidateAntiForgeryToken]
		public IActionResult ToggleStatus(int id)
		{
			var book = _context.Books.Find(id);
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
