namespace projekt.NET.Models.Entities
{
    public class GamePlatform
    {
        public int gameID {  get; set; }
        public Game? game { get; set; }
        public ICollection<Game> games { get; set; }
    }
}
