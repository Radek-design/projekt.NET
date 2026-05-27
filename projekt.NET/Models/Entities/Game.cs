using projekt.NET.Models;

namespace projekt.NET.Models.Entities
{
    public class Game
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public DateTime ReleaseDate { get; set; }

        // Ścieżka do okładki gry
        public string? CoverImagePath { get; set; }

        // Średnia ocen (poprawiona literówka)
        public double AverageRating { get; set; } = 0;

        // Relacja jeden do wielu z Producerem
        public int ProducerId { get; set; }
        public Producer? Producer { get; set; }

        // Relacje wiele do wielu (zauważ wielkie litery i poprawne nawiasy ())
        public ICollection<Platform> Platforms { get; set; } = new HashSet<Platform>();
        public ICollection<Genre> Genres { get; set; } = new HashSet<Genre>();

        // Relacja jeden do wielu z Review
        public ICollection<Review> Reviews { get; set; } = new HashSet<Review>();

        // Relacja do tabeli biblioteki użytkowników
        public ICollection<UserGame> UserGames { get; set; } = new HashSet<UserGame>();
    }
}