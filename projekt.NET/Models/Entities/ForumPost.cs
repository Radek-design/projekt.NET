using projekt.NET.Models.Entities;
using System.ComponentModel.DataAnnotations;

namespace projekt.NET.Models
{
    public class ForumPost
    {
        public int Id { get; set; }

        [Required]
        public string Title { get; set; } = string.Empty;

        [Required]
        public string Content { get; set; } = string.Empty;

        // Temat: "Dyskusja ogólna", "Problemy techniczne" lub nazwa/id gry
        public string Topic { get; set; } = string.Empty;
        public string? ImagePath { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // System moderacji
        public bool IsApproved { get; set; } = false; // Wpis czeka na zatwierdzenie
        public bool IsDeletedByModerator { get; set; } = false; // Oflagowanie usunięcia

        // Relacja z użytkownikiem (kto napisał post)
        public string UserId { get; set; } = string.Empty;
        public ApplicationUser? User { get; set; }
        public ICollection<ForumComment> Comments { get; set; } = new List<ForumComment>();
    }
}