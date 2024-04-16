namespace BIMS.Web.Seeds
{
	public class UsersList
	{
		public ApplicationUser ApplicationUser { get; set; } = null!;
		public string Password { get; set; } = null!;
		public List<string> Roles { get; set; } = new List<string>();
	}
}
