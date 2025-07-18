using System.Text.Json;
using AutoFiCore.Models;
using Microsoft.EntityFrameworkCore;

namespace AutoFiCore.Data;

public static class DbInitializer
{
    public static async Task InitializeAsync(IServiceProvider serviceProvider, IWebHostEnvironment env)
{
    using var scope = serviceProvider.CreateScope();
    var apiSettings = scope.ServiceProvider.GetRequiredService<ApiSettings>();
    
    // Skip if using mock API
    if (apiSettings.UseMockApi)
    {
        Console.WriteLine("Using mock API - skipping database initialization");
        return;
    }

    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    
    try
    {
        Console.WriteLine("Starting database migration...");
        
        // Test connection first
        await context.Database.CanConnectAsync();
        Console.WriteLine("Database connection successful");
        
        // Create database if it doesn't exist
        await context.Database.MigrateAsync();
        Console.WriteLine("Database migration completed successfully");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Database initialization failed: {ex.Message}");
        Console.WriteLine($"Stack trace: {ex.StackTrace}");
        throw;
    }
}
} 