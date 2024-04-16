namespace BIMS.Domain.Consts
{
	public static class Errors
	{
		public const string BlackListedSubscriber = "This subscriber is Black listed";
		public const string CopyIsInRental = "This copy is already rentaled.";
		public const string DenySpecialCharacters = "Special characters are not allowed.";
		public const string Duplicated = "Another record with the same {0} is already exists!";
		public const string DuplicatedBook = "Book with the same title is already exists with the same author!";
		public const string EmptyImage = "Please select an image.";
		public const string ExtendNotAllowed = "Rental cannot be extended.";
		public const string InactiveSubscriber = "This subscriber is Inactive";
		public const string InvalidRange = "{0} Should Br Between {1} and {2}";
		public const string InvalidUsername = "Username can only contain letters or digits.";
		public const string InvalidMobileNumber = "Invalid mobile number.";
		public const string InvalidNationalId = "Invalid national ID.";
		public const string InvalidSerialNumber = "Invalid serial number";
		public const string MaxMinLength = "The {0} must be at least {2} and at max {1} characters long.";
		public const string MaxLength = " Length cannot be more than {1} characters";
		public const string MaxSize = "File cannot be more than 2 MB!";
		public const string MaxCopiesReaches = "This subscriber reach max number of rentals";
		public const string NotAllowedExtensions = " only (.jpg,.jpeg,.png) files are allowed!";
		public const string NotAllowFutureDate = "Date Cannot be in the future";
		public const string NotAvailableRental = "This book/copy is not avilable for rental";
        public const string NotFoundSubscriber = "This subscriber is found.";
        public const string OnlyEnglishLetters = "Only English letters are allowed.";
		public const string OnlyArabicLetters = "Only Arabic letters are allowed.";
		public const string OnlyNumbersAndLetters = "Only Arabic/English letters or digits are allowed.";
		public const string PasswordAndConfirmNotMatch = "The password and confirmation password do not match.";
		public const string PenaltyShouldBePaid = "Penalty should be paid.";
		public const string RequiredField = "Required field";
		public const string RentalNotAlloweForBlackListed = "Rental cannot be extended for blacklisted subscribers";
		public const string RentalNotAlloweForNotActive = "Rental cannot be extended for this subscriber before renewal";
		public const string WeekPassword = "Passwords contain an uppercase character, lowercase character, a digit, and a non-alphanumeric character. Passwords must be at least 8 characters long";
		public const string ConfirmPasswordNotMatch = "The password and confirmation password do not match.";

	}
}
