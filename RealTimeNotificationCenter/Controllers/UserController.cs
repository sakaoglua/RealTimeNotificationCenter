using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using RealTimeNotificationCenter.Models;
using RealTimeNotificationCenter.Models.ViewModels;
using System.Diagnostics;

namespace RealTimeNotificationCenter.Controllers
{
    [Authorize]
    public class UserController(ILogger<UserController> logger, UserManager<IdentityUser> userManager, SignInManager<IdentityUser> signInManager) : Controller
    {
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
            var userToCreate = new IdentityUser()
            {
                UserName = model.Email,
                Email = model.Email
            };

            var result = await userManager.CreateAsync(userToCreate, model.Password);

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
            var hasUser = await userManager.FindByEmailAsync(model.Email);

            if (hasUser is null)
            {
                ModelState.AddModelError(string.Empty, "Email or password is incorrect");
                return View(model);
            }

            var result = await signInManager.PasswordSignInAsync(hasUser, model.Password, false, false);

            if (!result.Succeeded)
            {
                ModelState.AddModelError(string.Empty, "Email or password is incorrect");
                return View(model);
            }
            return RedirectToAction(nameof(Index));
        }
        [HttpPost]
        public async Task<IActionResult> SignOut()
        {
            await signInManager.SignOutAsync();

            return RedirectToAction(nameof(SignIn));
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
