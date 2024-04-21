using BaldurBillsApp.Models;
using BaldurBillsApp.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Net.Http;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddHttpClient<NbpRateService>();
builder.Services.AddHostedService<NbpRateBackgroundService>();

builder.Services.AddControllersWithViews().AddRazorRuntimeCompilation();

builder.Services.AddDbContext<BaldurBillsDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DESKTOP-AB6HGHF\\SQLEXPRESS;Database=BaldurBillsDB;Trusted_Connection=True;TrustServerCertificate=True")));

builder.Services.AddScoped<ISharedDataService, SharedDataService>();

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

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

//// Create a scope to get the NbpRateService
//using (var scope = app.Services.CreateScope())
//{
//    var services = scope.ServiceProvider;
//    try
//    {
//        // Get the service instance
//        var nbpRateService = services.GetRequiredService<NbpRateService>();
//        // Manually invoke the FetchAndSaveRatesAsync method
//        await nbpRateService.FetchAndSaveRatesAsync();
//    }
//    catch (Exception ex)
//    {
//        // Log or handle the exception as needed
//        var logger = services.GetRequiredService<ILogger<Program>>();
//        logger.LogError(ex, "An error occurred while fetching and saving rates.");
//    }
//}

app.Run();
