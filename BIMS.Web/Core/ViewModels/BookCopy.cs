namespace BIMS.Web.Core.ViewModels
{
	public class BookCopyViewModel
	{
		public int Id { get; set; }
		public string? bcKey { get; set; }
		public int BookId { get; set; }
		public string? BookTitle { get; set; }
		public string? BookThumbnailUrl { get; set; }
		[Display(Name = "Is Avilable For Rental?")]
		public bool IsAvailableForRental { get; set; }
		[Display(Name = "Edition Number")]
		[Range(1, 1000, ErrorMessage = Errors.InvalidRange)]
		public int EditionNumber { get; set; }
		public int SerialNumber { get; set; }
		public bool IsDeleted { get; set; }
		public DateTime CreatedOn { get; set; } = DateTime.Now;
		public bool ShowRentalInput { get; set; }

	}
}
