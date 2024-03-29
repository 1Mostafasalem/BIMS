namespace BIMS.Web.Core.Models
{
    [Index(nameof(Name), IsUnique = true)]
    public class Author : BaseModel
    {
        public int Id { get; set; }
        [MaxLength(200)]
        public string Name { get; set; } = null!; //Assign Deafult Value To Name (not null)
    }
}
