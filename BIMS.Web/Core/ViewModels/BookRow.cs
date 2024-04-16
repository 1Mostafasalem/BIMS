using UoN.ExpressiveAnnotations.NetCore.Attributes;

namespace BIMS.Web.Core.ViewModels
{
    public class BookRowViewModel
    {
        public int Id { get; set; }
        public string? bKey { get; set; }
        public string? Title { get; set; } = null!;
        public string Publisher { get; set; } = null!;
        public DateTime PublishingDate { get; set; } = DateTime.Now;
        public string? ImageThumbnailUrl { get; set; }
        public string Hall { get; set; } = null!;
        public bool IsAvilableForRental { get; set; }
        public string? Author { get; set; }
        public bool IsDeleted { get; set; }
        public IEnumerable<string>? CategoriesList { get; set; }
    }
}
