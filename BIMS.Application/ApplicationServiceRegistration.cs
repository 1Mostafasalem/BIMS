namespace BIMS.Infrastructure
{
	public static class ApplicationServiceRegistration
	{
		public static IServiceCollection AddApplicationServices(this IServiceCollection services)
		{
			services.AddScoped<IAreaService, AreaService>();
			services.AddScoped<IAuthorService, AuthorService>();
			services.AddScoped<IBookService, BookService>();
			services.AddScoped<IBookCopyService, BookCopyService>();
			services.AddScoped<ICategoryService, CategoryService>();
			services.AddScoped<IGovernoratesService, GovernoratesService>();
			services.AddScoped<IRentalService, RentalService>();
			services.AddScoped<ISubscriberService, SubscriberService>();
			services.AddScoped<IAuthService, AuthService>();

			return services;
		}
	}
}
