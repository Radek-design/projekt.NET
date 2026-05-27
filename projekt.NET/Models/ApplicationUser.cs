using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;

namespace projekt.NET.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string? FirstName { get; set; }
        public string? LastName { get; set; }

        // Relacja: Użytkownik może mieć wiele recenzji
        public virtual ICollection<Review> Reviews { get; set; } = new List<Review>();

        // Relacja: Użytkownik ma swoją bibliotekę gier (UserGames)
        public virtual ICollection<UserGame> UserGames { get; set; } = new List<UserGame>();
    }
}