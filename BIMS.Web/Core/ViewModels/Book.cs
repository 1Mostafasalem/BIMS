
using Newtonsoft.Json;
using UoN.ExpressiveAnnotations.NetCore.Attributes;

namespace Bookify.Web.Core.ViewModels
{
    public class BookViewModel
    {
        public int Id { get; set; }
        [MaxLength(100, ErrorMessage = Errors.MaxLength)]
        [Remote("AllowItem", null, AdditionalFields = "Id,AuthorId", ErrorMessage = Errors.DuplicatedBook)]
        //[RequiredIf("IsAvilableForRental == true",ErrorMessage = "Required field!")]
        [Required]
        public string? Title { get; set; } = null!;
        [Display(Name = "Author")]
        [Remote("AllowItem", null, AdditionalFields = "Id,Title", ErrorMessage = Errors.DuplicatedBook)]
        public int AuthorId { get; set; }

        public IEnumerable<SelectListItem>? Authors { get; set; }

        [MaxLength(200, ErrorMessage = Errors.MaxLength)]
        public string Publisher { get; set; } = null!;

        [Display(Name = "Publishing Date")]
        [AssertThat("PublishingDate <= Today()", ErrorMessage = Errors.NotAllowFutureDate)]
        public DateTime PublishingDate { get; set; } = DateTime.Now;

        public IFormFile? Image { get; set; }
        public string? ImageUrl { get; set; }
        public string? ImageThumbnailUrl { get; set; }
        public string? ImagePublicId { get; set; }

        [MaxLength(50, ErrorMessage = Errors.MaxLength)]
        public string Hall { get; set; } = null!;
        [Display(Name = "Is Avilable For Rental?")]
        public bool IsAvilableForRental { get; set; }
        [Required]
        public string Description { get; set; } = null!;

        [Required]
        [Display(Name = "Categories")]
        public IList<int> SelectedCategories { get; set; } = new List<int>();
        public IEnumerable<SelectListItem>? Categories { get; set; }
        public IEnumerable<string>? CategoriesList { get; set; }
        public string? Author { get; set; }
        public bool IsDeleted { get; set; }
        public IEnumerable<BookCopyViewModel> Copies { get; set; } = new List<BookCopyViewModel>();
        public DateTime CreatedOn { get; set; } = DateTime.Now;

    }
}
