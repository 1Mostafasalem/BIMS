using UoN.ExpressiveAnnotations.NetCore.Attributes;

namespace BIMS.Web.Core.ViewModels
{
	public class UserViewModel
	{
		public string? Id { get; set; }
		[MaxLength(100, ErrorMessage = Errors.MaxLength), Display(Name = "Full Name"),
			RegularExpression(RegexPatterns.CharactersOnly_Eng, ErrorMessage = Errors.OnlyEnglishLetters)]
		public string FullName { get; set; } = null!;

		[MaxLength(50, ErrorMessage = Errors.MaxLength), Display(Name = "Username"),
			Remote("AllowUserName", "Users", AdditionalFields = "Id", ErrorMessage = Errors.Duplicated),
			RegularExpression(RegexPatterns.Username, ErrorMessage = Errors.InvalidUsername)]
		public string UserName { get; set; } = null!;
		[EmailAddress, MaxLength(200, ErrorMessage = Errors.MaxLength),
			Remote("AllowEmail", "Users", AdditionalFields = "Id", ErrorMessage = Errors.Duplicated)]
		public string Email { get; set; } = null!;
		public bool IsDeleted { get; set; }
		public DateTime? CreatedOn { get; set; }
		public DateTime? LastUpdatedOn { get; set; }
		[DataType(DataType.Password),
			StringLength(100, ErrorMessage = Errors.MaxMinLength, MinimumLength = 8),
			RequiredIf("Id == null", ErrorMessage = Errors.RequiredField)]
		public string? Password { get; set; } = null!;
		[DataType(DataType.Password), Display(Name = "Confirm password"),
			Compare("Password", ErrorMessage = Errors.PasswordAndConfirmNotMatch),
			RegularExpression(RegexPatterns.Password, ErrorMessage = Errors.WeekPassword),
			RequiredIf("Id == null", ErrorMessage = Errors.RequiredField)]
		public string? ConfirmPassword { get; set; } = null!;
		public IEnumerable<SelectListItem>? Rolse { get; set; }
		[Display(Name = "Rolse")]
		public IList<string> RolseList { get; set; } = new List<string>();
	}
}
