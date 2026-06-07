using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using projekt.NET.Models.Entities;


namespace projekt.NET.Controllers
{
    public class AccountController : Controller
    {
        //UserManager  - zarządzanie użytkownikami(tworzenie, szukanie, itp)
        //SignInManager - zarządzanie logowaniem i wylogowywaniem
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly RoleManager<IdentityRole> _roleManager;


        public AccountController(UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
        }

        //GET: /Account/Login - wyświetla formularz logowania
        [HttpGet]
        public IActionResult Login() => View();


        //POST: /Account/Login - obsługuje dane z formularza logowania
        [HttpPost]
        public async Task<IActionResult> Login(string email, string password)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                ViewBag.Error = "Nieprawidłowy email lub hasło.";
                return View();

            }

            //Próbujemy się zalogować - false = nie pamiętaj mnie

            var result = await _signInManager.PasswordSignInAsync(user, password, false, false);
            if (result.Succeeded)
            {
                return RedirectToAction("Index", "Home");
            }
            else
            {
                ViewBag.Error = "Nieprawidłowy email lub hasło.";
                return View();
            }
        }

        //GET: /Account/Register - wyświetla formularz rejestracji
        [HttpGet]

        public IActionResult Register() => View();

        //POST: /Account/Register - tworzy nowe konto użytkownika

        [HttpPost]
        public async Task<IActionResult> Register(string email, string password, string displayname)
        {
            var user = new ApplicationUser
            {
                UserName = email,
                Email = email,
                DisplayName = displayname,
                CreatedAt = DateTime.UtcNow
            };
            var result = await _userManager.CreateAsync(user, password);
            if (result.Succeeded)
            {
                //upewniamy sie, że rola "User" istnieje, potem przypisujemy ją nowemu użytkownikowi
                if (!await _roleManager.RoleExistsAsync("User"))
                {
                    await _roleManager.CreateAsync(new IdentityRole("User"));
                }
                await _userManager.AddToRoleAsync(user, "User");    
                return RedirectToAction("Login", "Account");
            }
            //jeśli rejestracje się nie powiodła, wyświetlamy błąd
            else
            {
                ViewBag.Error = "Nie można utworzyć konta. Upewnij się, że hasło spełnia wymagania.";
                return View();
            }

        }
        //POST: /Account/Logout - wylogowuje użytkownika
        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");

        }

        //GET: /Account/AccessDenied - strona przy braku uprawnień
        public IActionResult AccessDenied() => View();

    }

}


