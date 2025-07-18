using AutoFiCore.BackgroundServices;
using AutoFiCore.Data;
using AutoFiCore.Hubs;
using AutoFiCore.Middleware;
using AutoFiCore.Models;
using AutoFiCore.Services;
using AutoFiCore.Utilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Polly;
using Polly.Retry;
using QuestPDF.Infrastructure;
using System.Text;

WebApplicationBuilder builder;
try
{
    builder = WebApplication.CreateBuilder(args);
}
catch (InvalidDataException ex) when (ex.Message.Contains("appsettings.json"))
{
    // Fallback: Create builder without appsettings.json if it's corrupted
    Console.WriteLine("WARNING: appsettings.json appears to be corrupted. Using environment variables only.");
    var options = new WebApplicationOptions
    {
        Args = args,
        ContentRootPath = AppContext.BaseDirectory
    };
    builder = WebApplication.CreateBuilder(options);
    
    // Add minimal configuration manually for Railway deployment
    builder.Configuration.AddInMemoryCollection(new Dictionary<string, string?>
    {
        ["Logging:LogLevel:Default"] = "Information",
        ["AllowedHosts"] = "*",
        ["ApiSettings:UseMockApi"] = "false",
        ["DatabaseSettings:ConnectionString"] = "",
        ["DatabaseSettings:MaxRetryCount"] = "3",
        ["DatabaseSettings:RetryDelaySeconds"] = "5",
        ["DatabaseSettings:CommandTimeoutSeconds"] = "30",
        ["Jwt:Secret"] = "",
        ["Jwt:Issuer"] = "AutoFiCore",
        ["Jwt:Audience"] = "AutoFiCoreClient",
        ["Jwt:ExpirationInMinutes"] = "60"
    });
}

QuestPDF.Settings.License = LicenseType.Community;

builder.Services.AddMemoryCache();

// Configure logging
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.SetMinimumLevel(LogLevel.Information);

var MyAllowSpecificOrigins = "_MyAllowSubdomainPolicy";

builder.Services.AddCors(options =>
{
    options.AddPolicy(MyAllowSpecificOrigins,
    policy =>
    {
        policy
            .WithOrigins(
                "http://localhost:3000"
            )
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials();
    });
});

// Configure database settings
var databaseSettings = builder.Configuration.GetSection("DatabaseSettings").Get<DatabaseSettings>()
    ?? throw new InvalidOperationException("Database settings are not configured properly.");

// Override with environment variables if available (for Railway deployment)
if (string.IsNullOrEmpty(databaseSettings.ConnectionString))
{
    var databaseUrl = Environment.GetEnvironmentVariable("DATABASE_URL");
    if (!string.IsNullOrEmpty(databaseUrl))
    {
        databaseSettings.ConnectionString = databaseUrl;
    }
}

builder.Services.AddSingleton(databaseSettings);

builder.Services.AddHealthChecks()
    .AddNpgSql(databaseSettings.ConnectionString, name: "database")
    .AddCheck<VehicleServiceHealthCheck>("vehicle-service");

// Configure API settings
var apiSettings = builder.Configuration.GetSection("ApiSettings").Get<ApiSettings>()
    ?? new ApiSettings { UseMockApi = false };
builder.Services.AddSingleton(apiSettings);

// Configure Entity Framework Core logging and DbContext
builder.Services.AddDbContext<ApplicationDbContext>((serviceProvider, options) =>
{
    var databaseSettings = serviceProvider.GetRequiredService<DatabaseSettings>();

    options.UseNpgsql(databaseSettings.ConnectionString, npgsqlOptions =>
    {
        npgsqlOptions.CommandTimeout(databaseSettings.CommandTimeoutSeconds);
        npgsqlOptions.EnableRetryOnFailure(
            maxRetryCount: databaseSettings.MaxRetryCount,
            maxRetryDelay: TimeSpan.FromSeconds(databaseSettings.RetryDelaySeconds),
            errorCodesToAdd: null);
    });

    // Only log SQL queries in development
    if (builder.Environment.IsDevelopment())
    {
        options.LogTo(Console.WriteLine, LogLevel.Information)
               .EnableSensitiveDataLogging()
               .EnableDetailedErrors();
    }
});

// Configure retry policy for database operations
var retryPolicy = Policy
    .Handle<DbUpdateException>()
    .Or<DbUpdateConcurrencyException>()
    .WaitAndRetryAsync(
        databaseSettings.MaxRetryCount,
        retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
        onRetry: (exception, timeSpan, retryCount, context) =>
        {
            // Log retry attempt
            Console.WriteLine($"Retry {retryCount} after {timeSpan.TotalSeconds} seconds due to: {exception.Message}");
        });

builder.Services.AddSingleton<IAsyncPolicy>(retryPolicy);

builder.Services.AddSingleton<TokenProvider>();

// Register repository based on configuration
if (apiSettings.UseMockApi)
{
    builder.Services.AddScoped<IVehicleRepository, MockVehicleRepository>();
}
else
{
    builder.Services.AddScoped<IVehicleRepository, DbVehicleRepository>();
    builder.Services.AddScoped<CachedVehicleRepository>();
    builder.Services.AddScoped<IContactInfoRepository, DbContactInfoRepository>();
    builder.Services.AddScoped<IUserRepository, DbUserRepository>();
    builder.Services.AddScoped<INewsLetterRepository, DbNewsLetterRepository>();
    builder.Services.AddScoped<IAuctionRepository, DbAuctionRepository>();
    builder.Services.AddScoped<IBidRepository, DbAuctionRepository>();
    builder.Services.AddScoped<IWatchlistRepository, DbAuctionRepository>();
    builder.Services.AddScoped<IAutoBidRepository, DbAutoBidRepository>();
}

// Register user service
builder.Services.AddScoped<IUserService, UserService>();

// Register loan service
builder.Services.AddScoped<ILoanService, LoanService>();

// Register Unit of work
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// Register email service
builder.Services.AddScoped<IEmailService, EmailService>();

// Register PDF service
builder.Services.AddScoped<IPdfService, PdfService>();

// Register vehicle service
builder.Services.AddScoped<IVehicleService, VehicleService>();

// Register vehicle health check service
builder.Services.AddScoped<VehicleServiceHealthCheck>();

// Register contact info service
builder.Services.AddScoped<IContactInfoService, ContactInfoService>();

// Register news letter service
builder.Services.AddScoped<INewsLetterService, NewsLetterService>();

// Register auction service
builder.Services.AddScoped<IAuctionService, AuctionService>();

// Register auto bid service
builder.Services.AddScoped<IAutoBidService, AutoBidService>();

// Register SignalR service
builder.Services.AddSignalR();

// Register auto bid background service
builder.Services.AddHostedService<AutoBidBackgroundService>();

// Register auction scheduler background service
builder.Services.AddHostedService<AuctionScheduler>();


// Add services to the container.
builder.Services.AddControllers();



// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
var jwtSecret = builder.Configuration["Jwt:Secret"];
// Override with environment variable if available (for Railway deployment)
if (string.IsNullOrEmpty(jwtSecret))
{
    jwtSecret = Environment.GetEnvironmentVariable("JWT_SECRET");
}
if (string.IsNullOrEmpty(jwtSecret))
    throw new InvalidOperationException("JWT Secret key is missing in configuration and JWT_SECRET environment variable.");

builder.Services.AddAuthentication("Bearer")
    .AddJwtBearer("Bearer", options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret)),
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };
    });

var app = builder.Build();

app.MapHealthChecks("/health");
app.MapHealthChecks("/health/ready");
app.MapHub<AuctionHub>("/hubs/auction");


StartupValidator.ValidateEnvironment(builder.Configuration, app.Environment);

app.UseMiddleware<GlobalExceptionMiddleware>();

// Initialize database
await DbInitializer.InitializeAsync(app.Services, app.Environment);


// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    var logger = app.Services.GetRequiredService<ILogger<Program>>();
    logger.LogInformation("OpenAPI Swagger UI available at: /swagger");
}

// Use CORS middleware
app.UseCors(MyAllowSpecificOrigins);

// Add our execution time logging middleware
app.UseRequestExecutionTimeLogging();

//app.UseHttpsRedirection();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();