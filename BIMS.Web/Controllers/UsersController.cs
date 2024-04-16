namespace BIMS.Web.Controllers
{
	[Authorize(Roles = AppRoles.Admin)]
	public class UsersController : Controller
	{
		private readonly UserManager<ApplicationUser> _userManager;
		private readonly RoleManager<IdentityRole> _roleManager;
		private readonly IMapper _mapper;
		private readonly IEmailSender _emailSender;
		private readonly IEmailBodyBuilder _emailBodyBuilder;
		private readonly IWebHostEnvironment _webHostEnvironment;
		private readonly IAuthService _authService;

		public UsersController(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager, IMapper mapper, IEmailSender emailSender, IWebHostEnvironment webHostEnvironment, IAuthService authService, IEmailBodyBuilder emailBodyBuilder)
		{
			_userManager = userManager;
			_roleManager = roleManager;
			_mapper = mapper;
			_emailSender = emailSender;
			_webHostEnvironment = webHostEnvironment;
			_authService = authService;
			_emailBodyBuilder = emailBodyBuilder;
		}

		public async Task<IActionResult> Index()
		{
			var users = await _authService.GetUsersAsync();
			return View(_mapper.Map<IEnumerable<UserViewModel>>(users));
		}

		[HttpGet]
		[AjaxOnly]
		public async Task<IActionResult> Create()
		{
			var roles = await _authService.GetRolesAsync();

			var model = new UserViewModel
			{
				Rolse = roles.Select(r => new SelectListItem { Text = r.Name, Value = r.Name })
			};


			return PartialView("_Create", model);
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Create(UserViewModel model)
		{
			if (!ModelState.IsValid)
				return BadRequest(model);
			try
			{


				var dto = _mapper.Map<CreateUserDto>(model);

				var result = await _authService.AddUserAsync(dto, User.GetUserId());

				if (result.IsSucceeded)
				{
					var user = result.User!;
					var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
					code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
					var callbackUrl = Url.Page(
						"/Account/ConfirmEmail",
						pageHandler: null,
						values: new { area = "Identity", userId = user.Id, code },
						protocol: Request.Scheme);

					var body = _emailBodyBuilder.GetEmailBody("https://res.cloudinary.com/mostafasalem/image/upload/v1675224866/welcome-icon_s9xapr.svg",
						$"Hey {user.FullName}, thanks for joining us!", "please confirm you email", $"{HtmlEncoder.Default.Encode(callbackUrl!)}", "Activate Account!");

					await _emailSender.SendEmailAsync(user.Email!, "Confirm your email", body);


					var newUser = _mapper.Map<UserViewModel>(user);
					return PartialView("_UserRow", newUser);
				}

				return BadRequest(string.Join(',', result.Errors!));
			}
			catch (Exception ex)
			{

				throw;
			}
		}

		public async Task<IActionResult> AllowUserName(UserViewModel model)
		{
			var user = await _userManager.FindByNameAsync(model.UserName);
			bool isAllowed = user is null || user.Id.Equals(model.Id);
			return Json(isAllowed);
		}

		public async Task<IActionResult> AllowEmail(UserViewModel model)
		{
			var user = await _userManager.FindByEmailAsync(model.Email);
			bool isAllowed = user is null || user.Id.Equals(model.Id);
			return Json(isAllowed);
		}

		[HttpPost]
		[AjaxOnly]
		public async Task<IActionResult> ToggleStatus(string Id)
		{
			var user = await _userManager.FindByIdAsync(Id);
			if (user is null)
				return BadRequest();

			user.IsDeleted = !user.IsDeleted;
			user.LastUpdatedById = User.FindFirst(ClaimTypes.NameIdentifier)!.Value;
			user.LastUpdatedOn = DateTime.Now;

			await _userManager.UpdateAsync(user);

			return Ok(user.LastUpdatedOn.ToString());
		}

		[HttpGet]
		[AjaxOnly]
		public async Task<IActionResult> ResetUserPassword(string Id)
		{
			var user = await _userManager.FindByIdAsync(Id);
			if (user is null)
				return NotFound();

			var model = new ResetPasswordViewModel() { Id = user.Id };

			return PartialView("_ResetPassword", model);
		}

		[HttpPost]
		[AjaxOnly]
		public async Task<IActionResult> ResetUserPassword(ResetPasswordViewModel model)
		{
			if (!ModelState.IsValid)
				return BadRequest(model);

			var user = await _userManager.FindByIdAsync(model.Id);
			if (user is null)
				return NotFound();

			var currentPasswordHash = user.PasswordHash;

			await _userManager.RemovePasswordAsync(user);

			var result = await _userManager.AddPasswordAsync(user, model.Password);

			if (result.Succeeded)
			{
				user.LastUpdatedOn = DateTime.Now;
				user.LastUpdatedById = User.FindFirst(ClaimTypes.NameIdentifier)!.Value;

				var userModel = _mapper.Map<UserViewModel>(user);
				return PartialView("_UserRow", userModel);
			}

			user.PasswordHash = currentPasswordHash;
			await _userManager.UpdateAsync(user);

			return BadRequest(string.Join(',', result.Errors.Select(e => e.Description)));
		}

		[HttpGet]
		[AjaxOnly]
		public async Task<IActionResult> Edit(string Id)
		{
			var user = await _userManager.FindByIdAsync(Id);

			if (user is null)
				return NotFound();

			var roles = await _userManager.GetRolesAsync(user);
			var model = _mapper.Map<UserViewModel>(user);

			model.Rolse = await _roleManager.Roles
				.Select(r => new SelectListItem
				{
					Text = r.Name,
					Value = r.Name
				})
				.ToListAsync();
			model.RolseList = await _userManager.GetRolesAsync(user);

			return PartialView("_Create", model);
		}

		[HttpPost]
		[AjaxOnly]
		public async Task<IActionResult> Edit(UserViewModel model)
		{
			if (!ModelState.IsValid)
				return BadRequest(model);

			var user = await _userManager.FindByIdAsync(model.Id);

			if (user is null)
				return NotFound();

			user = _mapper.Map(model, user);
			user.LastUpdatedOn = DateTime.Now;
			user.LastUpdatedById = User.FindFirst(ClaimTypes.NameIdentifier)!.Value;

			var result = await _userManager.UpdateAsync(user);
			if (result.Succeeded)
			{
				var currentRoles = await _userManager.GetRolesAsync(user);

				var rolesUpdated = !currentRoles.SequenceEqual(model.RolseList);
				if (rolesUpdated)
				{
					await _userManager.RemoveFromRolesAsync(user, currentRoles);
					await _userManager.AddToRolesAsync(user, model.RolseList);
				}
				var viewModel = _mapper.Map<UserViewModel>(user);
				return PartialView("_UserRow", viewModel);
			}
			return BadRequest(string.Join(',', result.Errors.Select(e => e.Description)));

		}

		[HttpPost]
		[AjaxOnly]
		public async Task<IActionResult> Unlock(string Id)
		{
			var user = await _userManager.FindByIdAsync(Id);
			if (user is null)
				return BadRequest();

			var isLocked = await _userManager.IsLockedOutAsync(user);

			if (isLocked)
				await _userManager.SetLockoutEndDateAsync(user, null);

			return Ok();
		}
	}
}