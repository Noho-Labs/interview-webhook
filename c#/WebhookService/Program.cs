using Microsoft.EntityFrameworkCore;
using WebhookService.Data;
using WebhookService.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? "Data Source=app.db";

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(connectionString));

builder.Services.AddScoped<OrdersService>();

var app = builder.Build();

// Startup: Initialize database (mirrors init_db() in python/app/db.py)
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.EnsureCreated();
}

// Handle seed command: dotnet run -- seed
if (args.Length > 0 && args[0] == "seed")
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await SeedData.RunAsync(db);
    return;
}

app.MapControllers();
app.Run("http://0.0.0.0:9000");

// Needed so WebApplicationFactory<Program> can reference this class from the test project
public partial class Program { }
