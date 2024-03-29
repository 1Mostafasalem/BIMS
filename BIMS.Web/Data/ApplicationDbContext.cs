using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace BIMS.Web.Data
{
	public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
	{
		public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
			: base(options)
		{
		}
		public DbSet<Author> Authors { get; set; }
		public DbSet<Book> Books { get; set; }
		public DbSet<BookCategory> BookCategories { get; set; }
		public DbSet<BookCopy> BookCopies { get; set; }
		public DbSet<Category> Categories { get; set; }

		protected override void OnModelCreating(ModelBuilder builder)
		{
			builder.HasSequence<int>("SerialNumber", schema: "shared")
				.StartsAt(1000001).IncrementsBy(1);

			builder.Entity<BookCopy>()
				.Property(e => e.SerialNumber).HasDefaultValueSql("NEXT VALUE FOR shared.SerialNumber");

			#region Set Delete Behavior to be Restrict
			builder.Entity<BookCategory>().HasKey(e => new { e.BookId, e.CategoryId });
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