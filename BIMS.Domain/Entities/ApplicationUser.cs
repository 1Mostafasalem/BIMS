namespace BIMS.Domain.Entities
{
	public class ApplicationUser : IdentityUser
	{
		public string FullName { get; set; }
		public bool IsDeleted { get; set; }
		public string? CreatedById { get; set; }
		[ForeignKey("CreatedById")]
		public ApplicationUser? CreatedBy { get; set; }
		public DateTime CreatedOn { get; set; }
		public string? LastUpdatedById { get; set; }
		public ApplicationUser? LastUpdatedBy { get; set; }
		public DateTime? LastUpdatedOn { get; set; }
	}
}
