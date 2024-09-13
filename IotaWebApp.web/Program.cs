using IotaWebApp.Data;
using IotaWebApp.Models;  // Include models if you use a separate file for DbInitializer
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = WebApplication.CreateBuilder(args);

// Register the DbContext with the dependency injection container and configure SQL Server
builder.Services.AddDbContext<WebsiteCMSDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Add services for controllers with views
builder.Services.AddControllersWithViews();

// Build the application
var app = builder.Build();

// Ensure the database is created and seed it with initial data
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var context = services.GetRequiredService<WebsiteCMSDbContext>();

    // Ensure the database is created and apply migrations if necessary
    context.Database.Migrate();

    // Seed the database with initial data
    DbInitializer.Initialize(context);  // Add this method in your DbInitializer.cs or in Program.cs itself
}

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();  // Enables detailed error pages in development mode
}
else
{
    app.UseExceptionHandler("/Home/Error");  // Handle exceptions in production mode
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles(); // Serve static files (CSS, JS, images, etc.)

app.UseRouting();  // Enable routing middleware

app.UseAuthorization();  // Enable authorization middleware

// Map the default controller route
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// Log when the application starts to ensure it's running properly
Console.WriteLine("Application has started and is running.");

app.Run();
