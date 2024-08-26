using BaldurBillsApp.Services;
using BaldurBillsApp.ViewModels;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BaldurBillsApp.Controllers
{
    public class AppUserController : Controller
    {
        private readonly UserService _userService;

        public AppUserController(UserService userService)
        {
            _userService = userService;
        }

        [HttpGet]
        public IActionResult Login()
        {
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Home");
            }
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _userService.Authenticate(model.UserName, model.Password);

            if (user == null)
            {
                ModelState.AddModelError("", "Invalid username or password.");
                return View(model);
            }
            var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, user.UserName)
                    // Możesz dodać więcej roszczeń w razie potrzeby
                };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

            var authProperties = new AuthenticationProperties
            {
                // Opcje ciasteczek, np. zapamiętanie logowania
                IsPersistent = true,
                ExpiresUtc = DateTimeOffset.UtcNow.AddMinutes(30)
            };

            // Ustawienie ciasteczka uwierzytelniającego
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity), authProperties);

            // Logika logowania - ustawienie cookie, przekierowanie, itp.
            return RedirectToAction("Index", "Home");
        }
    }
}
