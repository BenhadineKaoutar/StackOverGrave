using Microsoft.EntityFrameworkCore;
using StackOverGrave.Api.Data;
using StackOverGrave.Api.Services;

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
builder.Services.AddHttpClient<IAiConversionService, AiConversionService>();

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
        
        if (error != null)
        {
            logger.LogError(error.Error, "Unhandled exception");
        }
        
        context.Response.StatusCode = 500;
        context.Response.ContentType = "application/json";
        await context.Response.WriteAsJsonAsync(new { error = "An unexpected error occurred" });
    });
});

// Only redirect to HTTPS in production
if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseCors("AllowFrontend");
app.UseAuthorization();
app.MapControllers();

// Ensure database is created
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.EnsureCreated();
    
    // Check OpenAI API key configuration
    var config = scope.ServiceProvider.GetRequiredService<IConfiguration>();
    var apiKey = config["OpenAI:ApiKey"];
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    
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
