using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations.Schema;

namespace Bookify.Web.Core.Models
{
    [Index(nameof(Email), IsUnique = true)]
    [Index(nameof(UserName), IsUnique = true)]
    public class ApplicationUser : IdentityUser
    {
        [MaxLength(100)]
        public string FullName { get; set; }
        public bool IsDeleted { get; set; }
        public string? CreatedById { get; set; }
        [ForeignKey("CreatedById")]
        public ApplicationUser? CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; } = DateTime.Now;
        public string? LastUpdatedById { get; set; }
        public ApplicationUser? LastUpdatedBy { get; set; }
        public DateTime? LastUpdatedOn { get; set; }

    }
}
