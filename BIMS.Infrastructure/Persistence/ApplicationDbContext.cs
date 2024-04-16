using BIMS.Infrastructure.Persistence.Configurations;
using System.Reflection;

namespace BIMS.Infrastructure.Persistence
{
	public class ApplicationDbContext : IdentityDbContext<ApplicationUser>, IApplicationDbContext
	{
		public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
			: base(options)
		{
		}
		public DbSet<Area> Areas { get; set; }
		public DbSet<Author> Authors { get; set; }
		public DbSet<Book> Books { get; set; }
		public DbSet<BookCategory> BookCategories { get; set; }
		public DbSet<BookCopy> BookCopies { get; set; }
		public DbSet<Category> Categories { get; set; }
		public DbSet<Governorate> Governorates { get; set; }
		public DbSet<Rental> Rentals { get; set; }
		public DbSet<RentalCopy> RentalCopies { get; set; }
		public DbSet<Subscriber> Subscribers { get; set; }
		public DbSet<Subscription> Subscriptions { get; set; }

		protected override void OnModelCreating(ModelBuilder builder)
		{
			builder.HasSequence<int>("SerialNumber", schema: "shared")
				.StartsAt(1000001).IncrementsBy(1);

			builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

			builder.Entity<BookCopy>()
				.Property(e => e.SerialNumber).HasDefaultValueSql("NEXT VALUE FOR shared.SerialNumber");
			builder.Entity<BookCategory>().HasKey(e => new { e.BookId, e.CategoryId });
			builder.Entity<RentalCopy>().HasKey(e => new { e.RentalId, e.BookCopyId });
			builder.Entity<Rental>().HasQueryFilter(e => !e.IsDeleted);
			builder.Entity<RentalCopy>().HasQueryFilter(e => !e.Rental!.IsDeleted);
			#region Set Delete Behavior to be Restrict
			var fkCascade = builder.Model.GetEntityTypes()
				.SelectMany(t => t.GetForeignKeys())
				.Where(fk => !fk.IsOwnership && fk.DeleteBehavior == DeleteBehavior.Cascade);

			foreach (var fk in fkCascade)
				fk.DeleteBehavior = DeleteBehavior.Restrict;
			#endregion

			base.OnModelCreating(builder);

			//// Rename Identity Tables

			//builder.Entity<IdentityUser>().ToTable("Users", "security");
			//builder.Entity<IdentityRole>().ToTable("Roles", "security");
			//builder.Entity<IdentityUserRole<string>>().ToTable("UserRoles", "security");

			////Delete Column From Identity Table

			//builder.Entity<IdentityUser>()
			//       .Ignore(e => e.PhoneNumber)
			//       .Ignore(e => e.PhoneNumberConfirmed);


		}
	}
}