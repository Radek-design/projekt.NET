namespace projekt.NET.Models.Entities
{
    //encja slownikowa 
    //relacja jeden do wielu - jedna gra może mieć wiele gatunków
    public class Genre
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        //lista gier z tym gatunkiem 
        public ICollection<Game> Games { get; set; } = new List<Game>();
    }
}
