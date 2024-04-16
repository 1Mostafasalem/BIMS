﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using System.Text;
using System.Text.Encodings.Web;

namespace BIMS.Web.Areas.Identity.Pages.Account
{
	[AllowAnonymous]
	public class ResendEmailConfirmationModel : PageModel
	{
		private readonly UserManager<ApplicationUser> _userManager;
		private readonly IEmailSender _emailSender;
		private readonly IEmailBodyBuilder _emailBodyBuilder;


		public ResendEmailConfirmationModel(UserManager<ApplicationUser> userManager, IEmailSender emailSender, IEmailBodyBuilder emailBodyBuilder)
		{
			_userManager = userManager;
			_emailSender = emailSender;
			_emailBodyBuilder = emailBodyBuilder;
		}

		/// <summary>
		///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
		///     directly from your code. This API may change or be removed in future releases.
		/// </summary>
		[BindProperty]
		public InputModel Input { get; set; }

		/// <summary>
		///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
		///     directly from your code. This API may change or be removed in future releases.
		/// </summary>
		public class InputModel
		{
			/// <summary>
			///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
			///     directly from your code. This API may change or be removed in future releases.
			/// </summary>
			[Required]
			public string UserName { get; set; }
		}

		public void OnGet(string username)
		{
			Input = new()
			{
				UserName = username,
			};
		}

		public async Task<IActionResult> OnPostAsync()
		{
			if (!ModelState.IsValid)
			{
				return Page();
			}

			var normalizedUserName = Input.UserName.ToUpper();

			var user = await _userManager.Users.SingleOrDefaultAsync(u => (u.UserName == normalizedUserName || u.Email == normalizedUserName) && !u.IsDeleted);
			if (user == null)
			{
				ModelState.AddModelError(string.Empty, "Verification email sent. Please check your email.");
				return Page();
			}

			var userId = await _userManager.GetUserIdAsync(user);
			var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
			code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
			var callbackUrl = Url.Page(
				"/Account/ConfirmEmail",
				pageHandler: null,
				values: new { userId = userId, code = code },
			protocol: Request.Scheme);


			var body = _emailBodyBuilder.GetEmailBody(
				"https://res.cloudinary.com/mostafasalem/image/upload/v1675224866/welcome-icon_s9xapr.svg",
						$"Hey {user.FullName}, thanks for joining us!",
						"please confirm your email",
						$"{HtmlEncoder.Default.Encode(callbackUrl!)}",
						"Active Account!"
				);

			await _emailSender.SendEmailAsync(
				user.Email,
				"Confirm your email",
				body);

			ModelState.AddModelError(string.Empty, "Verification email sent. Please check your email.");
			return Page();
		}
	}
}
