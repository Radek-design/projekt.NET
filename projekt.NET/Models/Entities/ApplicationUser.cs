using projekt.NET.Models;
using Microsoft.AspNetCore.Identity;

namespace projekt.NET.Models.Entities
{
    public class ApplicationUser :IdentityUser{
    
        public string? DisplayName { get; set; }

        //Ścieżka do awatara profilowego użytkownika
        public string? AvatarPath { get; set; }

        // Data rejestracji konta
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // lista gier w bibliotece użytkownika
        //relacja jedend do wielu: jeden user -> wiele wpisów UserGame
        public ICollection<UserGame> UserGames { get; set; } = new List<UserGame>();
    }
}
