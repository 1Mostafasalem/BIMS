namespace BIMS.Web.Core.ViewModels
{
	public class ResetPasswordViewModel
	{
		public string? Id { get; set; } = null!;
		[Required, DataType(DataType.Password),
			StringLength(100, ErrorMessage = Errors.MaxMinLength, MinimumLength = 6)]
		public string Password { get; set; } = null!;
		[DataType(DataType.Password), Display(Name = "Confirm password"),
			Compare("Password", ErrorMessage = Errors.PasswordAndConfirmNotMatch),
			RegularExpression(RegexPatterns.Password, ErrorMessage = Errors.WeekPassword)]
		public string ConfirmPassword { get; set; } = null!;
	}
}
