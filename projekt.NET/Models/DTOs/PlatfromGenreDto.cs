namespace projekt.NET.Models.DTOs
{
    // Wspólne DTO dla Platform i Gatunków - mają taką samą strukturę
    public class PlatformGenreDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }
}