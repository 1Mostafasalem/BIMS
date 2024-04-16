var builder = WebApplication.CreateBuilder(args);

var assemblyConfigurationAttribute = Assembly.GetExecutingAssembly().GetCustomAttribute<AssemblyConfigurationAttribute>();
var buildConfiguration = assemblyConfigurationAttribute?.Configuration;

if (buildConfiguration == "Release")
{
	builder.Configuration
		.SetBasePath(Directory.GetCurrentDirectory())
		.AddJsonFile($"appsettings.Release.json", optional: true, reloadOnChange: true);
}

var app = builder.ConfigureServices()
				 .ConfigurePipeline();


var scopeFactory = app.Services.GetRequiredService<IServiceScopeFactory>();

using var scope = scopeFactory.CreateScope();

await app.SeedDatabaseAsync(scope);
await app.SeedRolesAndUsers(scope);

app.Run();
