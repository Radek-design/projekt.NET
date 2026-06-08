namespace projekt.NET.Models.DTOs
{
    // DTO - obiekt który wysyłamy do widoku
    // Zawiera tylko te dane które są potrzebne - nie wysyłamy całej encji z bazy
    public class GameDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public DateTime ReleaseDate { get; set; }
        public string? CoverImagePath { get; set; }
        public double AverageRating { get; set; }
        public string ProducerName { get; set; } = string.Empty;
        public int ProducerId { get; set; }
        public List<string> Platforms { get; set; } = new();
        public List<string> Genres { get; set; } = new();
    }

    // DTO do tworzenia i edycji gry - zawiera pola formularza
    public class CreateEditGameDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public DateTime ReleaseDate { get; set; }

        // Plik okładki przesyłany przez formularz HTML
        public IFormFile? CoverImage { get; set; }
        public string? CoverImagePath { get; set; }

        // ID wybranego producenta z listy rozwijanej
        public int ProducerId { get; set; }

        // Lista zaznaczonych platform i gatunków (checkboxy)
        public List<int> SelectedPlatformIds { get; set; } = new();
        public List<int> SelectedGenreIds { get; set; } = new();
    }
}