using System.ComponentModel.DataAnnotations;

namespace projekt.NET.Models
{
    public class Review
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Content { get; set; } = string.Empty;

        [Range(1, 10)]
        public int Rating { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Relacja z grą (zakładając, że masz model Game lub UserGame)
        public int GameId { get; set; }

        // Relacja z użytkownikiem
        public string UserId { get; set; } = string.Empty;
        public ApplicationUser? User { get; set; }
    }
}