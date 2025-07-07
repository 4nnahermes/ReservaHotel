using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ReservaHotel.Data;
using ReservaHotel.Areas.Identity.Data;
var builder = WebApplication.CreateBuilder(args);
var connectionString = builder.Configuration.GetConnectionString("ReservaHotelContextConnection") ?? throw new InvalidOperationException("Connection string 'ReservaHotelContextConnection' not found.");

builder.Services.AddDbContext<ReservaHotelContext>(options => options.UseSqlServer(connectionString));

builder.Services.AddDefaultIdentity<ReservaHotelUser>(options => options.SignIn.RequireConfirmedAccount = false).AddEntityFrameworkStores<ReservaHotelContext>();

// Add services to the container.
builder.Services.AddControllersWithViews();

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

app.MapRazorPages();
app.Run();
