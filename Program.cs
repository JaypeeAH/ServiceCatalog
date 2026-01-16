using Microsoft.EntityFrameworkCore;
using ServiceCatalog.Web.Data;
using ServiceCatalog.Web.Commands;

// Check for CSV import command
if (args.Length > 0 && args[0] == "import-csv")
{
    await ImportCsvCommand.RunAsync(args);
    return;
}

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Add API Controllers with JSON options
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
        options.JsonSerializerOptions.DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
    });

// Add Swagger/OpenAPI documentation
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "Cardinal Health Service Catalog API",
        Version = "v1",
        Description = "RESTful API for managing Cardinal Health Service Catalog data including Capabilities, Services, Processes, and Locations",
        Contact = new Microsoft.OpenApi.Models.OpenApiContact
        {
            Name = "Cardinal Health",
            Email = "support@cardinalhealth.com"
        }
    });
    
    // Include XML comments if available
    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        options.IncludeXmlComments(xmlPath);
    }
});

// Add CORS policy for API access
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        builder => builder
            .AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader());
});

// Add DbContext with SQL Server
builder.Services.AddDbContext<ServiceCatalogDbContext>(options =>
  options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

var app = builder.Build();

// Ensure database is created and migrations are applied
try
{
 using (var scope = app.Services.CreateScope())
    {
     var services = scope.ServiceProvider;
        var context = services.GetRequiredService<ServiceCatalogDbContext>();
        var logger = services.GetRequiredService<ILogger<Program>>();
        
    logger.LogInformation("====================================");
        logger.LogInformation("DATABASE STATUS CHECK");
        logger.LogInformation("====================================");
        
        // Check if database exists
        var canConnect = await context.Database.CanConnectAsync();
   
      if (!canConnect)
        {
      logger.LogWarning("??  Database doesn't exist. Creating...");
   await context.Database.MigrateAsync();
  logger.LogInformation("? Database created successfully!");
        }
        else
 {
            // Check for pending migrations
    var pendingMigrations = await context.Database.GetPendingMigrationsAsync();
 if (pendingMigrations.Any())
 {
            logger.LogInformation("Applying pending migrations...");
           try
    {
       await context.Database.MigrateAsync();
        logger.LogInformation("? Migrations applied successfully!");
      }
   catch (Exception ex)
   {
    logger.LogWarning($"Migration warning (may be safe to ignore): {ex.Message}");
          // Continue anyway - tables might already exist
         }
   }
            else
        {
        logger.LogInformation("? Database is up to date!");
            }
   }
    
      // Display database statistics
  try
        {
            var capabilityCount = await context.Capabilities.CountAsync();
    var serviceCount = await context.Services.CountAsync();
            var processCount = await context.ServiceProcesses.CountAsync();
var applicabilityCount = await context.ServiceApplicabilities.CountAsync();
        var locationCount = await context.Locations.CountAsync();
        
 logger.LogInformation("");
            logger.LogInformation("Database Statistics:");
    logger.LogInformation($"  - Capabilities: {capabilityCount}");
            logger.LogInformation($"  - Services: {serviceCount}");
            logger.LogInformation($"  - Processes: {processCount}");
            logger.LogInformation($"  - Applicabilities: {applicabilityCount}");
            logger.LogInformation($"  - Locations: {locationCount}");
            logger.LogInformation("");
        
            if (serviceCount == 0)
         {
   logger.LogWarning("??  No services found in database!");
         logger.LogInformation("To import data, run: dotnet run import-csv");
     }
            else
  {
 logger.LogInformation("? Database has data - ready to use!");
       }
        }
        catch (Exception ex)
{
          logger.LogError($"Error checking database statistics: {ex.Message}");
     }
        
   logger.LogInformation("====================================");
    }
}
catch (Exception ex)
{
    var logger = app.Services.GetRequiredService<ILogger<Program>>();
    logger.LogError(ex, "An error occurred during database initialization.");
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}
else
{
    // Enable Swagger only in development
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Service Catalog API v1");
        options.RoutePrefix = "api/docs"; // Access at /api/docs
        options.DocumentTitle = "Cardinal Health Service Catalog API";
        options.DefaultModelsExpandDepth(-1); // Hide schemas section by default
    });
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// Enable CORS
app.UseCors("AllowAll");

app.UseAuthorization();

// Map API Controllers
app.MapControllers();

// Map MVC Controllers
app.MapControllerRoute(
 name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
