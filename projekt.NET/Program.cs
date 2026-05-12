using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using projekt.NET.Data;
using projekt.NET.Models.Entities;

var builder = WebApplication.CreateBuilder(args);

// === BAZA DANYCH ===
// £¹czymy aplikacjê z SQL Server przez connection string z appsettings.json
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(connectionString));

// === IDENTITY (logowanie i role) ===
// Konfigurujemy system logowania - u¿ytkownik to ApplicationUser, role to IdentityRole
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 6;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = false;
})
.AddEntityFrameworkStores<AppDbContext>()
.AddDefaultTokenProviders();

// === PRZEKIEROWANIA ===
// Gdzie kierowaæ u¿ytkownika przy logowaniu, wylogowaniu i braku uprawnieñ
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.LogoutPath = "/Account/Logout";
    options.AccessDeniedPath = "/Account/AccessDenied";
});

// === AUTOMAPPER ===
// Biblioteka do mapowania obiektów np. encji na DTO i odwrotnie
builder.Services.AddAutoMapper(typeof(Program));

builder.Services.AddControllersWithViews();

var app = builder.Build();


if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
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

app.Run();