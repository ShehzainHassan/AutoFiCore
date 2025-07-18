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
                "http://localhost:3000",
                "http://localhost:3001", 
                "http://localhost:5173",
                "http://localhost:5174",
                "http://localhost:4200",
                "http://127.0.0.1:3000",
                "http://127.0.0.1:3001",
                "http://127.0.0.1:5173",
                "http://127.0.0.1:5174",
                "http://127.0.0.1:4200"
            )
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials();
    });
});

// Configure API settings first to determine if we need database
var apiSettings = builder.Configuration.GetSection("ApiSettings").Get<ApiSettings>()
    ?? new ApiSettings { UseMockApi = false };
builder.Services.AddSingleton(apiSettings);

// Configure database settings only if not using mock API
DatabaseSettings databaseSettings = new DatabaseSettings();
if (!apiSettings.UseMockApi)
{
    databaseSettings = builder.Configuration.GetSection("DatabaseSettings").Get<DatabaseSettings>()
        ?? throw new InvalidOperationException("Database settings are not configured properly.");

    // Override with environment variables if available (for Railway deployment)
    if (string.IsNullOrEmpty(databaseSettings.ConnectionString))
    {
        var databaseUrl = Environment.GetEnvironmentVariable("DATABASE_URL");
        if (!string.IsNullOrEmpty(databaseUrl))
        {
            try
            {
                // Convert Railway DATABASE_URL format to Npgsql connection string
                databaseSettings.ConnectionString = ConvertRailwayDatabaseUrl(databaseUrl);
                Console.WriteLine($"Using converted DATABASE_URL for connection");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error converting DATABASE_URL: {ex.Message}");
                Console.WriteLine($"Original DATABASE_URL format: {databaseUrl}");
                throw new InvalidOperationException($"Failed to convert DATABASE_URL to valid connection string: {ex.Message}");
            }
        }
        else
        {
            Console.WriteLine("No DATABASE_URL environment variable found");
            throw new InvalidOperationException("No database connection string configured. Set DATABASE_URL environment variable or configure ConnectionString in appsettings.json");
        }
    }

    // Add health checks with proper error handling
    builder.Services.AddHealthChecks()
        .AddNpgSql(databaseSettings.ConnectionString, name: "database", timeout: TimeSpan.FromSeconds(10))
        .AddCheck<VehicleServiceHealthCheck>("vehicle-service", timeout: TimeSpan.FromSeconds(5));

    // Log connection string info for debugging (without sensitive data)
    Console.WriteLine($"Database connection configured - Host: {GetHostFromConnectionString(databaseSettings.ConnectionString)}");
}
else
{
    // Mock API mode - add basic health checks without database
    builder.Services.AddHealthChecks()
        .AddCheck<VehicleServiceHealthCheck>("vehicle-service", timeout: TimeSpan.FromSeconds(5));
    
    Console.WriteLine("Using Mock API - database connection not required");
}

builder.Services.AddSingleton(databaseSettings);

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

// Configure Railway port binding
var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
var railwayUrl = $"http://0.0.0.0:{port}";

// Override ASPNETCORE_URLS for Railway deployment
if (!string.IsNullOrEmpty(Environment.GetEnvironmentVariable("RAILWAY_ENVIRONMENT")))
{
    builder.WebHost.UseUrls(railwayUrl);
    Console.WriteLine($"Railway deployment detected - binding to {railwayUrl}");
}
else if (!string.IsNullOrEmpty(Environment.GetEnvironmentVariable("PORT")))
{
    builder.WebHost.UseUrls(railwayUrl);
    Console.WriteLine($"PORT environment variable detected - binding to {railwayUrl}");
}

var app = builder.Build();

// Configure health check endpoints with detailed logging
app.MapHealthChecks("/health", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
{
    ResponseWriter = async (context, report) =>
    {
        var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();
        
        context.Response.ContentType = "application/json";
        
        var response = new
        {
            status = report.Status.ToString(),
            checks = report.Entries.Select(x => new
            {
                name = x.Key,
                status = x.Value.Status.ToString(),
                description = x.Value.Description,
                duration = x.Value.Duration.TotalMilliseconds
            }),
            totalDuration = report.TotalDuration.TotalMilliseconds
        };
        
        logger.LogInformation("Health check completed with status: {Status}", report.Status);
        
        await context.Response.WriteAsync(System.Text.Json.JsonSerializer.Serialize(response));
    }
});

app.MapHealthChecks("/health/ready", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("ready"),
    ResponseWriter = async (context, report) =>
    {
        var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();
        logger.LogInformation("Readiness check completed with status: {Status}", report.Status);
        
        context.Response.ContentType = "text/plain";
        await context.Response.WriteAsync(report.Status.ToString());
    }
});

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

// Helper method to convert Railway DATABASE_URL to Npgsql connection string
static string ConvertRailwayDatabaseUrl(string databaseUrl)
{
    if (string.IsNullOrEmpty(databaseUrl))
    {
        throw new ArgumentException("Database URL cannot be null or empty");
    }

    try
    {
        // Parse the DATABASE_URL (format: postgresql://username:password@host:port/database)
        var uri = new Uri(databaseUrl);
        
        if (uri.Scheme != "postgresql" && uri.Scheme != "postgres")
        {
            throw new ArgumentException($"Invalid database URL scheme: {uri.Scheme}. Expected 'postgresql' or 'postgres'");
        }

        // Extract components
        var host = uri.Host;
        var port = uri.Port != -1 ? uri.Port : 5432;
        var database = uri.AbsolutePath.TrimStart('/');
        var username = uri.UserInfo?.Split(':')[0];
        var password = uri.UserInfo?.Split(':').Length > 1 ? uri.UserInfo.Split(':')[1] : "";

        // Handle URL decoding for special characters
        if (!string.IsNullOrEmpty(password))
        {
            password = Uri.UnescapeDataString(password);
        }
        if (!string.IsNullOrEmpty(username))
        {
            username = Uri.UnescapeDataString(username);
        }
        if (!string.IsNullOrEmpty(database))
        {
            database = Uri.UnescapeDataString(database);
        }

        // Build Npgsql connection string
        var connectionString = $"Host={host};Port={port};Database={database};Username={username};Password={password};";
        
        // Add SSL settings for production (Railway requires SSL)
        connectionString += "SSL Mode=Require;Trust Server Certificate=true;";

        return connectionString;
    }
    catch (UriFormatException ex)
    {
        throw new ArgumentException($"Invalid DATABASE_URL format: {ex.Message}", ex);
    }
    catch (Exception ex)
    {
        throw new ArgumentException($"Error parsing DATABASE_URL: {ex.Message}", ex);
    }
}

// Helper method to safely extract host from connection string for logging
static string GetHostFromConnectionString(string connectionString)
{
    try
    {
        var parts = connectionString.Split(';');
        var hostPart = parts.FirstOrDefault(p => p.StartsWith("Host=", StringComparison.OrdinalIgnoreCase));
        return hostPart?.Split('=')[1] ?? "Unknown";
    }
    catch
    {
        return "Unknown";
    }
}