using Microsoft.EntityFrameworkCore;
using StackOverGrave.Api.Data;
using StackOverGrave.Api.Services;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Database
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection") 
        ?? "Data Source=stackovergrave.db"));

// Services
builder.Services.AddScoped<ITechnologyDetectionService, TechnologyDetectionService>();
builder.Services.AddScoped<IAiConversionService, AiConversionService>();
builder.Services.AddScoped<IFileStorageService, FileStorageService>();
builder.Services.AddScoped<IPackagingService, PackagingService>();
builder.Services.AddScoped<IGitRepositoryService, GitRepositoryService>();
builder.Services.AddScoped<IRepositoryAnalysisService, RepositoryAnalysisService>();
builder.Services.AddScoped<IRepositoryConversionService, RepositoryConversionService>();
builder.Services.AddScoped<IMigrationGuideService, MigrationGuideService>();
builder.Services.AddSingleton<IRepositoryJobProcessor, RepositoryJobProcessor>();
builder.Services.AddHttpClient<IAiConversionService, AiConversionService>();
builder.Services.AddHttpClient<IGitRepositoryService, GitRepositoryService>();
builder.Services.AddHttpClient<IMigrationGuideService, MigrationGuideService>();

// CORS for frontend - Allow all localhost origins in development
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.SetIsOriginAllowed(origin => 
                new Uri(origin).Host == "localhost")
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});

// Rate limiting for API endpoints
var rateLimitConfig = builder.Configuration.GetSection("RateLimiting");
if (rateLimitConfig.GetValue<bool>("EnableRateLimiting"))
{
    builder.Services.AddRateLimiter(options =>
    {
        options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
            RateLimitPartition.GetFixedWindowLimiter(
                partitionKey: context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                factory: partition => new FixedWindowRateLimiterOptions
                {
                    PermitLimit = rateLimitConfig.GetValue<int>("PermitLimit"),
                    Window = TimeSpan.FromSeconds(rateLimitConfig.GetValue<int>("WindowSeconds")),
                    QueueLimit = rateLimitConfig.GetValue<int>("QueueLimit")
                }));
        
        options.OnRejected = async (context, cancellationToken) =>
        {
            context.HttpContext.Response.StatusCode = 429;
            await context.HttpContext.Response.WriteAsJsonAsync(new
            {
                error = "Too many requests",
                suggestion = "Please wait a moment before trying again"
            }, cancellationToken);
        };
    });
}

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Global exception handler
app.UseExceptionHandler(errorApp =>
{
    errorApp.Run(async context =>
    {
        var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();
        var error = context.Features.Get<Microsoft.AspNetCore.Diagnostics.IExceptionHandlerFeature>();
        
        context.Response.ContentType = "application/json";
        
        if (error?.Error is StackOverGrave.Api.Exceptions.RepositoryException repoEx)
        {
            // Handle RepositoryException with user-friendly messages
            logger.LogWarning(repoEx, "Repository operation failed: {Message}", repoEx.UserMessage);
            
            context.Response.StatusCode = repoEx.StatusCode;
            await context.Response.WriteAsJsonAsync(new
            {
                error = repoEx.UserMessage,
                details = repoEx.Details,
                suggestion = repoEx.Suggestion
            });
        }
        else if (error?.Error != null)
        {
            // Handle unexpected exceptions without exposing stack traces
            logger.LogError(error.Error, "Unhandled exception: {Message}", error.Error.Message);
            
            context.Response.StatusCode = 500;
            await context.Response.WriteAsJsonAsync(new
            {
                error = "An unexpected error occurred",
                suggestion = "Please try again or contact support if the problem persists"
            });
        }
        else
        {
            context.Response.StatusCode = 500;
            await context.Response.WriteAsJsonAsync(new { error = "An unexpected error occurred" });
        }
    });
});

// Only redirect to HTTPS in production
if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseCors("AllowFrontend");

// Enable rate limiting if configured
if (builder.Configuration.GetSection("RateLimiting").GetValue<bool>("EnableRateLimiting"))
{
    app.UseRateLimiter();
}

app.UseAuthorization();
app.MapControllers();

// Apply database migrations and check configuration
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    
    try
    {
        logger.LogInformation("🔄 Applying database migrations...");
        db.Database.Migrate();
        logger.LogInformation("✅ Database migrations applied successfully");
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "❌ Failed to apply database migrations");
        throw;
    }
    
    // Check OpenAI API key configuration
    var config = scope.ServiceProvider.GetRequiredService<IConfiguration>();
    var apiKey = config["OpenAI:ApiKey"];
    
    if (string.IsNullOrWhiteSpace(apiKey))
    {
        logger.LogWarning("⚠️  OpenAI API key is not configured! Code conversion will fail.");
        logger.LogWarning("⚠️  Please add your OpenAI API key to appsettings.json under 'OpenAI:ApiKey'");
    }
    else
    {
        logger.LogInformation("✅ OpenAI API key is configured (length: {Length})", apiKey.Length);
    }
}

app.Run();
