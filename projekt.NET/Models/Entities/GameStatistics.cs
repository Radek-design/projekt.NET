namespace projekt.NET.Models.Entities
{
    public class GameStatistics
    {
        public int GameId { get; set; }
        public string Title { get; set; } = string.Empty;
        public int OwnersCount { get; set; }
        public double AverageRating { get; set; }
        public int ReviewCount { get; set; }
    }
}