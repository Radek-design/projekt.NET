namespace projekt.NET.Models.Entities
{
    
    
    
    public class Game
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;

        public string? Description { get; set; }

        public DateTime ReleaseDate { get; set; }   

        //sciezka do okładki gry
        public string? CoverImagePath { get; set; }

        //srednia aktualizowana przez trigger w bazie danych po dodaniu lub aktualizacji oceny w tabeli UserGame
        public double AvarageRating { get; set; } = 0;
        
        //relacja jeden do wielu z Producerem - jedna gra ma jednego producenta, ale jeden producent może mieć wiele gier
        public int ProducerId { get; set; }

        public Producer? Producer { get; set; }

        //relacja wiele do wielu z Platform - jedna gra może być na wielu platformach, a jedna platforma może mieć wiele gier
        public ICollection<Platform> Platforms { get; set; } = new List<Platform>();

        //relacja wiele do wielu z Genre - jedna gra może mieć wiele gatunków, a jeden gatunek może być przypisany do wielu gier
        public ICollection<Genre> Genres { get; set; } = new List<Genre>();

        //relacja jeden do wielu z Review - jedna gra może mieć wiele recenzji, ale jedna recenzja jest przypisana do jednej gry
        public ICollection<Review>Reviews { get; set; } = new List<Review>();


    }

}
