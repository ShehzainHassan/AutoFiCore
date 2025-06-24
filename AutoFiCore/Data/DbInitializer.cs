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
            return;
        }

        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        
        // Create database if it doesn't exist
        await context.Database.MigrateAsync();
    }
} 