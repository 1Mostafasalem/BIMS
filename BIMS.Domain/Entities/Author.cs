namespace BIMS.Domain.Entities
{
	public class Author : BaseEntity
	{
		public int Id { get; set; }
		public string Name { get; set; } = null!; //Assign Deafult Value To Name (not null)
	}
}
