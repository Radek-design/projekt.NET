using projekt.NET.Models.Entities;

namespace projekt.NET.Models
{
    public class UserGame
    {
        public int Id { get; set; }

        // Relacja z użytkownikiem (kto dodał grę)
        public string UserId { get; set; } = string.Empty;
        public ApplicationUser? User { get; set; }

        // Relacja z grą (jaka gra została dodana)
        public int GameId { get; set; }
        public Game? Game { get; set; }

        // Dane specyficzne dla kolekcji użytkownika
        public string Status { get; set; } = string.Empty; // Ukończona, W trakcie, Porzucona
        public int? Rating { get; set; } // Indywidualna ocena użytkownika (1-10)
        public int PlayTimeHours { get; set; }
    }
}