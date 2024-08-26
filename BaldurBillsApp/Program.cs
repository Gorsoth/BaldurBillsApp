using BaldurBillsApp.Models;
using BaldurBillsApp.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Hosting.Internal;
using System.Net.Http;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Rejestracja UserService
builder.Services.AddScoped<UserService>();

builder.Services.AddHttpClient<NbpRateService>();
builder.Services.AddHostedService<NbpRateBackgroundService>();

builder.Services.AddControllersWithViews().AddRazorRuntimeCompilation();

builder.Services.AddDbContext<BaldurBillsDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DESKTOP-AB6HGHF\\SQLEXPRESS;Database=BaldurBillsDB;Trusted_Connection=True;TrustServerCertificate=True")));


// Add authentication services
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/AppUser/Login";  // Adjust the path according to your login route
        options.AccessDeniedPath = "/AppUser/AccessDenied";  // Optional: path for access denied
    });

// Add authorization services
builder.Services.AddAuthorization();
builder.Services.AddScoped<ISharedDataService, SharedDataService>();

builder.Services.AddScoped<PdfService>();
builder.Services.AddAutoMapper(typeof(Program));


var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.UseEndpoints(endpoints =>
{
    endpoints.MapControllerRoute(
        name: "default",
        pattern: "{controller=AppUser}/{action=Login}/{id?}");
});

// Create a scope to get the NbpRateService
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        // Get the service instance
        var nbpRateService = services.GetRequiredService<NbpRateService>();
        // Manually invoke the FetchAndSaveRatesAsync method
        await nbpRateService.FetchAndSaveRatesAsync();
    }
    catch (Exception ex)
    {
        // Log or handle the exception as needed
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while fetching and saving rates.");
    }
}

app.Run();
