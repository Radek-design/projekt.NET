namespace projekt.NET.Models.Entities
{
    public class Review
    {
        public int Id { get; set; }

        
        public string Content { get; set; } = string.Empty; //treść recenzji
        public int Rating { get; set; } //ocena od 1 do 10  

        public DateTime CreateAt { get; set; } = DateTime.Now; //data dodania recezji
        public int GameId { get; set; }

        public Game? Game { get; set; }


        public string UserID { get; set; } = string.Empty; //id użytkownika który dodał recenzję
         public ApplicationUser? User { get; set; }
    }
}
