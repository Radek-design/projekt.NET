

using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using projekt.NET.Models.Entities;

namespace projekt.NET.Data
{
    public class AppDbContext : IdentityDbContext<ApplicationUser>
    {
        // Konstruktor
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        // Każda linia DbSet<T> to jedna tabela w bazie

        public DbSet<Game> Games { get; set; }
        public DbSet<Producer> Producers { get; set; }
        public DbSet<Platform> Platforms { get; set; }
        public DbSet<Genre> Genres { get; set; }
        public DbSet<Review> Reviews { get; set; }

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