namespace projekt.NET.Models.Entities
{
    public class GameGenre
    {
        // Pośrednia tabela pomiędzy gra a gatunkiem
        public int gameID { get; set; }
        public Game? game { get; set; }
        // relacja wiele do wielu gra gatunek jedna gra ma wiele gatunków
        public ICollection<Game> games { get; set; } = new HashSet<Game>();
        public int genreID { get; set; }
        public Genre? genre { get; set; }
        // relacja wiele do wielu gatunek gra jeden gatunek do wielu gier
        public ICollection<Genre> genres { get; set; } = new List<Genre>();
    }
}
