namespace projekt.NET.Models.Entities
{
    //relacja wiele do wielu - jedna gra może być na wielu platformach, a jedna platforma może mieć wiele gier
    public class Platfrom
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        //lista gier na tej platformie 
        public ICollection<Game> Games { get; set; } = new List<Game>();
    }
}
