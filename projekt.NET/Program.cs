using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using projekt.NET.Data;
using projekt.NET.Models.Entities;
using projekt.NET.Repositories.Interface;
using projekt.NET.Repositories.Implementations;
using projekt.NET.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHttpClient<RawgService>();


// === REPOZYTORIA ===
// Scoped = nowa instancja na ka¿de ¿¹danie HTTP
builder.Services.AddScoped<IGameRepository, GameRepository>();
builder.Services.AddScoped<IProducerRepository, ProducerRepository>();
builder.Services.AddScoped<IPlatformRepository, PlatformRepository>();
builder.Services.AddScoped<IGenreRepository, GenreRepository>();

//BAZA DANYCH
// £¹czymy aplikacjê z SQL Server przez connection string z appsettings.json
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(connectionString));

//IDENTITY (logowanie i role)
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

//PRZEKIEROWANIA
// Gdzie kierowaæ u¿ytkownika przy logowaniu, wylogowaniu i braku uprawnieñ
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.LogoutPath = "/Account/Logout";
    options.AccessDeniedPath = "/Account/AccessDenied";
});

//AUTOMAPPER
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


// Tworzymy domyœlne role jeœli nie istniej¹
using (var scope = app.Services.CreateScope())
{
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    foreach (var role in new[] { "User", "Moderator" })
    {
        if (!await roleManager.RoleExistsAsync(role))
            await roleManager.CreateAsync(new IdentityRole(role));
    }
}

app.Run();