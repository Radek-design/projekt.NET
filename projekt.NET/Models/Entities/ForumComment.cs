using projekt.NET.Models.Entities;

namespace projekt.NET.Models
{
    public class ForumComment
    {
        public int Id { get; set; }
        public string Content { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public int ForumPostId { get; set; }
        public ForumPost? ForumPost { get; set; }

        public string UserId { get; set; } = string.Empty;
        public ApplicationUser? User { get; set; }
    }
}