using System;

namespace projekt.NET.Models.Entities
{
    public class Premiere
    {
        public int Id { get; set; }

        public string Title { get; set; } = string.Empty;
        public DateTime ReleaseDate { get; set; }
        public string Platforms { get; set; } = string.Empty;
        public string Genres { get; set; } = string.Empty;
    }
}