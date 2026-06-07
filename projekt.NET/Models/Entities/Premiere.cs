using System;

namespace projekt.NET.Models.Entities
{
    public class Premiere
    {
        public int Id { get; set; }

        // Relacja i klucz obcy powiązany bezpośrednio z tabelą gier (Game)
        public int GameId { get; set; }
        public virtual Game? Game { get; set; }
    }
}