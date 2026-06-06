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

<<<<<<< HEAD
        // Relacje wiele do wielu (zauważ wielkie litery i poprawne nawiasy ())
        public ICollection<Platform> Platforms { get; set; } = new HashSet<Platform>();
        public ICollection<Genre> Genres { get; set; } = new HashSet<Genre>();
=======
        //relacja wiele do wielu z Platform - jedna gra może być na wielu platformach, a jedna platforma może mieć wiele gier
        public ICollection<Platform> Platforms { get; set; } = new List<Platform>();

        //relacja wiele do wielu z Genre - jedna gra może mieć wiele gatunków, a jeden gatunek może być przypisany do wielu gier
        public ICollection<Genre> Genres { get; set; } = new List<Genre>();

        //relacja jeden do wielu z Review - jedna gra może mieć wiele recenzji, ale jedna recenzja jest przypisana do jednej gry
        public ICollection<Review>Reviews { get; set; } = new List<Review>();
>>>>>>> michal

        // Relacja jeden do wielu z Review
        public ICollection<Review> Reviews { get; set; } = new HashSet<Review>();

        // Relacja do tabeli biblioteki użytkowników
        public ICollection<UserGame> UserGames { get; set; } = new HashSet<UserGame>();
    }
}