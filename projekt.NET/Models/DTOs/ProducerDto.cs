namespace projekt.NET.Models.DTOs
{
    public class ProducerDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Country { get; set; } = string.Empty;
        public string? LogoPath { get; set; }
        public IFormFile? LogoFile { get; set; }
        public int GameCount { get; set; }
    }
}