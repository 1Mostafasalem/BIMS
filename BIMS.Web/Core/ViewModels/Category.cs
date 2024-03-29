namespace Bookify.Web.Core.ViewModels
{
    public class CategoryViewModel
    {
        public int Id { get; set; }
        [MaxLength(100, ErrorMessage = Errors.MaxLength)]
        [Display(Name = "Category")]
        [Remote("AllowItem", null, AdditionalFields = "Id", ErrorMessage = Errors.Duplicated)]
        public string Name { get; set; } = null!;
        public bool IsDeleted { get; set; }
        public DateTime CreatedOn { get; set; } = DateTime.Now;
        public DateTime? LastUpdatedOn { get; set; }
    }
}
