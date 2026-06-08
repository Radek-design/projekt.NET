using projekt.NET.Models.Entities;

namespace projekt.NET.Models
{
    public class ForumComment
    {
        public int Id { get; set; }
        public string Content { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Klucz obcy - do jakiego wpisu należy ten komentarz
        public int ForumPostId { get; set; }
        public ForumPost? ForumPost { get; set; }

        // Klucz obcy - kto napisał ten komentarz
        public string UserId { get; set; } = string.Empty;
        public ApplicationUser? User { get; set; }
    }
}