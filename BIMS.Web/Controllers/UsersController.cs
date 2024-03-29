using Bookify.Web.Filters;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Text;
using static System.Net.WebRequestMethods;

namespace Bookify.Web.Controllers
{
    [Authorize(Roles = AppRoles.Admin)]
    public class UsersController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IMapper _Mapper;
        private readonly ApplicationDbContext _context;
        private readonly IEmailSender _emailSender;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public UsersController(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager, IMapper Mapper, ApplicationDbContext context, IEmailSender emailSender, IWebHostEnvironment webHostEnvironment)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _Mapper = Mapper;
            _context = context;
            _emailSender = emailSender;
            _webHostEnvironment = webHostEnvironment;
        }

        public async Task<IActionResult> Index()
        {
            var users = await _userManager.Users.ToListAsync();
            var model = _Mapper.Map<IEnumerable<UserViewModel>>(users);
            return View(model);
        }

        [HttpGet]
        [AjaxOnly]
        public async Task<IActionResult> Create()
        {
            var model = new UserViewModel
            {
                Rolse = await _roleManager.Roles
                .Select(r => new SelectListItem
                {
                    Text = r.Name,
                    Value = r.Name
                })
                .ToListAsync()
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
                ApplicationUser user = new()
                {
                    FullName = model.FullName,
                    UserName = model.UserName,
                    Email = model.Email,
                    CreatedById = User.FindFirst(ClaimTypes.NameIdentifier)!.Value
                };

                var result = await _userManager.CreateAsync(user, model.Password);
                if (result.Succeeded)
                {
                    await _userManager.AddToRolesAsync(user, model.RolseList);

                    var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                    code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
                    var callbackUrl = Url.Page(
                        "/Account/ConfirmEmail",
                        pageHandler: null,
                        values: new { area = "Identity", userId = user.Id, code },
                        protocol: Request.Scheme);

                    var filePath = $"{_webHostEnvironment.WebRootPath}/Templates/Email.html";

                    StreamReader str = new StreamReader(filePath);
                    var body = await str.ReadToEndAsync();
                    str.Close();

                    //// Another Way To Read Text In File 
                    //var body = System.IO.File.ReadAllText(filePath);


                    //var url = Url.Action("Index", "Home", null, Request.Scheme);

                    var imageUrl = "https://res.cloudinary.com/mostafasalem/image/upload/v1675224866/welcome-icon_s9xapr.svg";

                    body = body.Replace("[imageUrl]", imageUrl)
                        .Replace("[Header]", $"Hey {user.FullName}, thanks for joining us!")
                        .Replace("[body]", "please confirm you email")
                        .Replace("[url]", $"{HtmlEncoder.Default.Encode(callbackUrl!)}")
                        .Replace("[LinkTitle]", "Activate Account!");


                    await _emailSender.SendEmailAsync(user.Email, "Confirm your email", body);


                    var newUser = _Mapper.Map<UserViewModel>(user);
                    return PartialView("_UserRow", newUser);
                }

                return BadRequest(string.Join(',', result.Errors.Select(e => e.Description)));
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

                var userModel = _Mapper.Map<UserViewModel>(user);
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
            var model = _Mapper.Map<UserViewModel>(user);

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

            user = _Mapper.Map(model, user);
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
                var viewModel = _Mapper.Map<UserViewModel>(user);
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