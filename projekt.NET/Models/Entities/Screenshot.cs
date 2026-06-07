using projekt.NET.Models.Entities;

namespace projekt.NET.Models
{
    public class Screenshot
    {
        public int Id { get; set; }

        // Ścieżka do pliku na serwerze / w chmurze
        public string ImagePath { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Relacja z użytkownikiem
        public string UserId { get; set; } = string.Empty;
        public ApplicationUser? User { get; set; }

        // Relacja z grą (do której gry przypisano screena)
        public int GameId { get; set; }
        public Game? Game { get; set; }
    }
}