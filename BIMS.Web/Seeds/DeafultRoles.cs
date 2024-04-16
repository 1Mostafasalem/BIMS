namespace BIMS.Web.Seeds
{
	public static class DeafultRoles
	{
		public static async Task SeedAsync(RoleManager<IdentityRole> roleManager)
		{
			if (!roleManager.Roles.Any())
			{
				await roleManager.CreateAsync(new IdentityRole(AppRoles.Admin));
				await roleManager.CreateAsync(new IdentityRole(AppRoles.Archive));
				await roleManager.CreateAsync(new IdentityRole(AppRoles.Reception));
			}
		}
	}
}
