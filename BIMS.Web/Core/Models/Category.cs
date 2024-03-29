namespace Bookify.Web.Core.Models
{
    [Index(nameof(Name), IsUnique = true)]
    public class Category : BaseModel
    {
        public int Id { get; set; }
        [MaxLength(200)]
        public string Name { get; set; } = null!; //Assign Deafult Value To Name (not null)
        public ICollection<BookCategory> Books { get; set; } = new List<BookCategory>();

    }
}
