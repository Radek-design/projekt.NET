using Microsoft.AspNetCore.Mvc;

namespace projekt.NET.Controllers
{
    public class ForumController : Controller
    {
        // Główne wejście do sekcji forum
        public IActionResult Index()
        {
            return View();
        }
    }
}