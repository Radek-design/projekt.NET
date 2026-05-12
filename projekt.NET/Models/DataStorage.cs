using projekt.NET.Models;

namespace projekt.NET.Data
{
    public static class DataStorage
    {
        // Tutaj lądują Twoje gry z LibraryController
        public static List<UserGame> MyGames = new List<UserGame>
        {
            new UserGame { Id = 1,
                Title = "Wiedźmin 3: Dziki Gon",
                Status = "Ukończona",
                Rating = 10,
                PlayTimeHours = 120,
                ImageUrl = "https://images.igdb.com/igdb/image/upload/t_cover_big/co1wyy.jpg"},
            new UserGame { Id = 2, Title = "Cyberpunk 2077",
                Status = "W trakcie",
                Rating = 8,
                PlayTimeHours = 45,
                ImageUrl = "https://images.igdb.com/igdb/image/upload/t_cover_big/co2mvt.jpg"},
            new UserGame { Id = 3,
                Title = "Gothic",
                Status = "Ukończona",
                Rating = 9,
                PlayTimeHours = 60,
                ImageUrl = "https://images.igdb.com/igdb/image/upload/t_cover_big/co2k9x.jpg"}
        };
    }
}