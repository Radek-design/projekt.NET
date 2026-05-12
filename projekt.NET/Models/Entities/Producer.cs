namespace projekt.NET.Models.Entities
{
    //encja Producent - jeden producent może wydać wiele gier (relacja jeden do wielu)
    public class Producer
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;
        public string Country { get; set; } = string.Empty;

        //przechowujemy ścieżkę do loga, nie sam plik
        public string? LogoPath { get; set; }

        //lista gier produenta - ef automatycznie wypelnia ta liste 
        public ICollection<Game> Games { get; set; } = new List<Game>();
    }
}
