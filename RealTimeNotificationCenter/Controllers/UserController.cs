using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using RealTimeNotificationCenter.Models;
using RealTimeNotificationCenter.Models.ViewModels;
using System.Diagnostics;

namespace RealTimeNotificationCenter.Controllers
{
    [Authorize]
    public class UserController : Controller
    {
        private readonly ILogger<UserController> _logger;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;

        public UserController(ILogger<UserController> logger, UserManager<IdentityUser> userManager, SignInManager<IdentityUser> signInManager)
        {
            _logger = logger;
            _userManager = userManager;
            _signInManager = signInManager;
        }

        [Authorize]
        public IActionResult Index()
        {
            return View();
        }

        [AllowAnonymous]
        public IActionResult SignUp()
        {
            return View();
        }
        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> SignUp(SignUpViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            if (!string.Equals(model.Password, model.ConfirmPassword, StringComparison.Ordinal))
            {
                ModelState.AddModelError(string.Empty, "Şifreler eşleşmiyor.");
                return View(model);
            }

            var userToCreate = new IdentityUser()
            {
                UserName = model.UserName.Trim(),
                Email = model.Email.Trim()
            };

            var result = await _userManager.CreateAsync(userToCreate, model.Password);

            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
                return View(model);
            }
            return RedirectToAction(nameof(SignIn));
        }
        [AllowAnonymous]
        public IActionResult SignIn()
        {
            return View();
        }
        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> SignIn(SignInViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var userNameOrEmail = model.UserNameOrEmail.Trim();
            var hasUser = await _userManager.FindByNameAsync(userNameOrEmail)
                ?? await _userManager.FindByEmailAsync(userNameOrEmail);

            if (hasUser is null)
            {
                ModelState.AddModelError(string.Empty, "Kullanıcı adı/email veya şifre yanlış.");
                return View(model);
            }

            var result = await _signInManager.PasswordSignInAsync(hasUser, model.Password, false, false);

            if (!result.Succeeded)
            {
                ModelState.AddModelError(string.Empty, "Kullanıcı adı/email veya şifre yanlış.");
                return View(model);
            }
            return RedirectToAction(nameof(Index));
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public new async Task<IActionResult> SignOut()
        {
            await _signInManager.SignOutAsync();

            return RedirectToAction(nameof(SignIn));
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
