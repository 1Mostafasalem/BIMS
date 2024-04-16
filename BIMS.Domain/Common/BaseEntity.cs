﻿namespace BIMS.Domain.Common
{
	public class BaseEntity
	{
		public bool IsDeleted { get; set; }
		public string? CreatedById { get; set; }
		[ForeignKey("CreatedById")]
		public ApplicationUser? CreatedBy { get; set; }
		public string? LastUpdatedById { get; set; }
		public ApplicationUser? LastUpdatedBy { get; set; }
		public DateTime CreatedOn { get; set; } = DateTime.Now;
		public DateTime? LastUpdatedOn { get; set; }
	}
}
