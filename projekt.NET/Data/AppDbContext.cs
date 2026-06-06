using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using projekt.NET.Models;
using projekt.NET.Models.Entities;

namespace projekt.NET.Data
{

    public class AppDbContext : IdentityDbContext<ApplicationUser>
    {
        // Konstruktor
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

    // Rejestracja tabel w bazie danych
    public DbSet<Game> Games { get; set; }
    public DbSet<Producer> Producers { get; set; }
    public DbSet<Platform> Platforms { get; set; }
    public DbSet<Genre> Genres { get; set; }
    public DbSet<Review> Reviews { get; set; }
    public DbSet<UserGame> UserGames { get; set; } // Dodana brakująca tabela!

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Konfiguracja relacji wiele do wielu: Gry <-> Platformy
            modelBuilder.Entity<Game>()
                .HasMany(g => g.Platforms)
                .WithMany(p => p.Games);

            // Konfiguracja relacji wiele do wielu: Gry <-> Gatunki
            modelBuilder.Entity<Game>()
                .HasMany(g => g.Genres)
                .WithMany(g => g.Games);

            // Konfiguracja relacji jeden do wielu: Producent -> Gry
            modelBuilder.Entity<Game>()
                .HasOne(g => g.Producer)
                .WithMany(p => p.Games)
                .HasForeignKey(g => g.ProducerId);

            // Konfiguracja relacji dla tabeli łączącej UserGames
            modelBuilder.Entity<UserGame>()
                .HasOne(ug => ug.User)
                .WithMany(u => u.UserGames)
                .HasForeignKey(ug => ug.UserId);

            modelBuilder.Entity<UserGame>()
                .HasOne(ug => ug.Game)
                .WithMany(g => g.UserGames)
                .HasForeignKey(ug => ug.GameId);

            // Unikalny indeks: Użytkownik może dodać daną grę do biblioteki tylko raz
            modelBuilder.Entity<UserGame>()
                .HasIndex(ug => new { ug.UserId, ug.GameId })
                .IsUnique();
        }
        // Każda linia DbSet<T> to jedna tabela w bazie


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            //wywołujemy konfigurację tabel Identity
            base.OnModelCreating(modelBuilder);

            //konfiguracja relacji wiele do wielu 
            modelBuilder.Entity<Game>()
                .HasMany(g => g.Platforms)
                .WithMany(p => p.Games);


            modelBuilder.Entity<Game>()
                .HasMany(g => g.Genres)
                .WithMany(g => g.Games);

            //relacja jeden do wielu 
            modelBuilder.Entity<Game>()
                .HasOne(g => g.Producer)
                .WithMany(p => p.Games)
                .HasForeignKey(g => g.ProducerId);
        }
    }
}