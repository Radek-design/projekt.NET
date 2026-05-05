namespace projekt.NET.Models
{
    public class UserGame
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Status { get; set; } // Zgodnie z projektem: Ukończona, W trakcie, Porzucona
        public int? Rating { get; set; } // Ocena w skali 1-10
        public int PlayTimeHours { get; set; }
        public string ImageUrl { get; set; }
    }
}