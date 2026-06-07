using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using projekt.NET.Models;
using projekt.NET.Models.Entities;

namespace projekt.NET.Data
{
    // Dziedziczenie po IdentityDbContext, aby obsłużyć użytkowników i role
    public class AppDbContext : IdentityDbContext<ApplicationUser>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        // Rejestracja tabel w bazie danych
        public DbSet<Game> Games { get; set; } = null!;
        public DbSet<Producer> Producers { get; set; } = null!;
        public DbSet<Platform> Platforms { get; set; } = null!;
        public DbSet<Genre> Genres { get; set; } = null!;
        public DbSet<Review> Reviews { get; set; } = null!;
        public DbSet<UserGame> UserGames { get; set; } = null!; // Dodana brakująca tabela!
        public DbSet<ForumPost> ForumPosts { get; set; } = null!;
        public DbSet<Screenshot> Screenshots { get; set; } = null!;
        public DbSet<Premiere> Premieres { get; set; } = null!;
        public DbSet<ForumComment> ForumComments { get; set; } = null!;

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
            // Relacja ForumPost -> User
            modelBuilder.Entity<ForumPost>()
                .HasOne(fp => fp.User)
                .WithMany(u => u.ForumPosts)
                .HasForeignKey(fp => fp.UserId);

            // Relacja Screenshot -> User
            modelBuilder.Entity<Screenshot>()
                .HasOne(s => s.User)
                .WithMany(u => u.Screenshots)
                .HasForeignKey(s => s.UserId)
                .OnDelete(DeleteBehavior.NoAction); // Zapobiega błędom cyklicznego kaskadowego usuwania

            // Relacja Screenshot -> Game
            modelBuilder.Entity<Screenshot>()
                .HasOne(s => s.Game)
                .WithMany(g => g.Screenshots)
                .HasForeignKey(s => s.GameId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ForumComment>()
            .HasOne(fc => fc.ForumPost)
            .WithMany(fp => fp.Comments) // Musisz dodać kolekcję w ForumPost.cs
            .HasForeignKey(fc => fc.ForumPostId)
            .OnDelete(DeleteBehavior.Cascade);
        }
    }
}