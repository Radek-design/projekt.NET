using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;

namespace projekt.NET.Models
{
    // Jeśli używasz Identity, dziedzicz po IdentityUser
    public class ApplicationUser : IdentityUser
    {
        // Tutaj możesz dodać dodatkowe pola dla użytkownika
        public string? FirstName { get; set; }
        public string? LastName { get; set; }

        // Relacja: Użytkownik może mieć wiele recenzji
        public virtual ICollection<Review> Reviews { get; set; } = new List<Review>();
    }
}