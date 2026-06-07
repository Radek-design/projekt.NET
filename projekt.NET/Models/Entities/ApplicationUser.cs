using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;
using System;

namespace projekt.NET.Models.Entities
{
    public class ApplicationUser : IdentityUser
    {
        public string? DisplayName { get; set; }

        // Ścieżka do awatara profilowego użytkownika
        public string? AvatarPath { get; set; }

        // Data rejestracji konta
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Właściwości przeniesione z usuniętego pliku
        public string? FirstName { get; set; }
        public string? LastName { get; set; }

        // Relacja: Użytkownik ma swoją bibliotekę gier (UserGames)
        public virtual ICollection<UserGame> UserGames { get; set; } = new List<UserGame>();

        // Relacja: Użytkownik może mieć wiele recenzji
        public virtual ICollection<Review> Reviews { get; set; } = new List<Review>();
    }
}