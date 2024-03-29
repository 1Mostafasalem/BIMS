using BIMS.Web.Core.Mapping;
using BIMS.Web.Seeds;
using Microsoft.AspNetCore.Identity;
using System.Reflection;
using UoN.ExpressiveAnnotations.NetCore.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using BIMS.Web.Data;
using BIMS.Web.Helpers;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity.UI.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
	options.UseSqlServer(connectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

//builder.Services.AddDefaultIdentity<ApplicationUser>(options => options.SignIn.RequireConfirmedAccount = true)
//    .AddEntityFrameworkStores<ApplicationDbContext>();

builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options => options.SignIn.RequireConfirmedAccount = true)
	.AddEntityFrameworkStores<ApplicationDbContext>()
	.AddDefaultUI()
	.AddDefaultTokenProviders();

builder.Services.Configure<IdentityOptions>(options =>
{
	// Password
	options.Password.RequiredLength = 8;

	// User
	options.User.RequireUniqueEmail = true;

	//Lockout
	options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
	options.Lockout.MaxFailedAccessAttempts = 5;
});


//Cookie Setting

builder.Services.ConfigureApplicationCookie(options =>
{
	//    options.AccessDeniedPath = "/Identity/Account/AccessDenied";
	//    options.Cookie.Name = "YourAppCookieName";
	//    options.Cookie.HttpOnly = true;
	options.ExpireTimeSpan = TimeSpan.FromDays(1);
	//    options.LoginPath = "/Identity/Account/Login";
	//    // ReturnUrlParameter requires 
	//    //using Microsoft.AspNetCore.Authentication.Cookies;
	//    options.ReturnUrlParameter = CookieAuthenticationDefaults.ReturnUrlParameter;
	//    options.SlidingExpiration = true;
});

builder.Services.AddControllersWithViews();

builder.Services.AddAutoMapper(Assembly.GetAssembly(typeof(MappingProfile)));

// Register Configuration
builder.Services.Configure<CloudinarySetting>(builder.Configuration.GetSection("CloudinarySettings"));
builder.Services.Configure<MailSetting>(builder.Configuration.GetSection("MailSettings"));

builder.Services.AddExpressiveAnnotations();

// Register Services
builder.Services.AddScoped<IUserClaimsPrincipalFactory<ApplicationUser>, ApplicationUserClaimsPrincipalFactory>();
builder.Services.AddTransient<IImageService, ImageService>();
builder.Services.AddTransient<IEmailSender, EmailSender>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
	app.UseMigrationsEndPoint();
}
else
{
	app.UseExceptionHandler("/Home/Error");
	// The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
	app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

var scopeFactory = app.Services.GetRequiredService<IServiceScopeFactory>();

using var scope = scopeFactory.CreateScope();

try
{
	var context = scope.ServiceProvider.GetService<ApplicationDbContext>();
	if (context != null)
	{
		await context.Database.EnsureDeletedAsync();
		await context.Database.MigrateAsync();
	}
}
catch (Exception ex)
{
	// Add Logger 
}

var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

await DeafultRoles.SeedAsync(roleManager);
await DefaultUsers.SeedAdminUserAsync(userManager);



app.MapControllerRoute(
	name: "default",
	pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages();

app.Run();
