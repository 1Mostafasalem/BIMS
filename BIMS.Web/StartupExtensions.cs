using BIMS.Web.Core.Mapping;
using BIMS.Web.Helpers;
using BIMS.Web.Seeds;
using Microsoft.AspNetCore.Identity.UI.Services;
using Serilog;
using Serilog.Context;
using System.Reflection;
using UoN.ExpressiveAnnotations.NetCore.DependencyInjection;

namespace BIMS.Web
{
	public static class StartupExtensions
	{
		public static WebApplication ConfigureServices(this WebApplicationBuilder builder)
		{
			// Add services to the container.
			builder.Services.AddInfrastructureServices(builder.Configuration);
			builder.Services.AddApplicationServices();

			// Add services to the container.

			var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

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
				options.ExpireTimeSpan = TimeSpan.FromMinutes(60);
				//    options.LoginPath = "/Identity/Account/Login";
				//    // ReturnUrlParameter requires 
				//    //using Microsoft.AspNetCore.Authentication.Cookies;
				//    options.ReturnUrlParameter = CookieAuthenticationDefaults.ReturnUrlParameter;
				//    options.SlidingExpiration = true;
			});

			builder.Services.AddControllersWithViews();

			builder.Services.AddAutoMapper(Assembly.GetAssembly(typeof(MappingProfile)));

			// Register Configuration
			builder.Services.Configure<MailSetting>(builder.Configuration.GetSection("MailSettings"));

			builder.Services.AddExpressiveAnnotations();

			// Register Services
			builder.Services.AddDataProtection().SetApplicationName(nameof(BIMS));
			builder.Services.AddScoped<IUserClaimsPrincipalFactory<ApplicationUser>, ApplicationUserClaimsPrincipalFactory>();
			builder.Services.AddTransient<IImageService, ImageService>();
			builder.Services.AddTransient<IEmailSender, EmailSender>();
			builder.Services.AddTransient<IEmailBodyBuilder, EmailBodyBuilder>();

			//Auto Apply AntiforgeryTokenAttribute
			builder.Services.AddMvc(options =>
			{
				options.Filters.Add(new AutoValidateAntiforgeryTokenAttribute());
			});

			//Add Serilog
			Log.Logger = new LoggerConfiguration().ReadFrom.Configuration(builder.Configuration).CreateLogger();
			builder.Host.UseSerilog();

			return builder.Build();
		}
		public static WebApplication ConfigurePipeline(this WebApplication app)
		{
			// Configure the HTTP request pipeline.
			if (app.Environment.IsDevelopment())
			{
				//app.UseExceptionHandler(errorHandlingPath: "/Home/Error");
				app.UseMigrationsEndPoint();
			}
			else
			{
				app.UseExceptionHandler("/Home/Error");
				// The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
				app.UseHsts();
			}


			//Send Status Code To Error Handler to dynamic change image 
			app.UseStatusCodePagesWithReExecute("/Home/Error", "?statusCode={0}");

			app.UseHttpsRedirection();
			app.UseStaticFiles();

			//Focre Secure Cookie
			// I Disable this only here because MosnsterApp Hosting not contain SSL certificate 
			app.UseCookiePolicy(new CookiePolicyOptions
			{
				//Secure = CookieSecurePolicy.Always,
			});

			//Force Disallow IFrame
			app.Use(async (context, next) =>
			{
				context.Response.Headers.Append("X-Frame-Options", "Deny");
				await next();
			});

			app.UseRouting();

			app.UseAuthentication();
			app.UseAuthorization();


			// Add UserId and UserName to Log  Table  
			app.Use(async (context, next) =>
			{
				LogContext.PushProperty("UserId", context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
				LogContext.PushProperty("UserName", context.User.FindFirst(ClaimTypes.Name)?.Value);

				await next();
			});

			app.UseSerilogRequestLogging();

			app.MapControllerRoute(
				name: "default",
				pattern: "{controller=Home}/{action=Index}/{id?}");

			app.MapRazorPages();

			return app;
		}

		public static async Task SeedDatabaseAsync(this WebApplication app, IServiceScope scope)
		{
			try
			{
				var context = scope.ServiceProvider.GetService<ApplicationDbContext>();
				if (context != null)
				{
					//await context.Database.EnsureDeletedAsync();
					await context.Database.MigrateAsync();
					var scriptsDirectory = Path.Combine(Directory.GetCurrentDirectory(), "Seeds", "Scripts");
					if (Directory.Exists(scriptsDirectory))
					{
						var scripts = Directory.GetFiles(scriptsDirectory);
						foreach (var script in scripts)
						{
							var sql = File.ReadAllText(script);
							context.Database.ExecuteSqlRaw(sql);
						}
					}
				}
			}
			catch (Exception ex)
			{
				// Add Logger 
			}
		}

		public static async Task SeedRolesAndUsers(this WebApplication app, IServiceScope scope)
		{
			var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
			var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

			await DeafultRoles.SeedAsync(roleManager);
			await DefaultUsers.SeedAdminUserAsync(userManager);
		}
	}
}
