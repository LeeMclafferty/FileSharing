using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using FileSharing.Models;
using FileSharing.Services;
using Microsoft.AspNetCore.Identity.UI.Services;
using FileSharing.Interfaces;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authentication.Cookies;
using NuGet.Versioning;

namespace FileSharing.Controllers
{
    public class AccountController : Controller
    {

        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IEmailSender _emailSender;
        private readonly IRenderService _renderService;

        public AccountController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, IEmailSender emailSender,
            IRenderService renderService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _emailSender = emailSender;
            _renderService = renderService;
        }

        /* GET */
        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpGet]
        public IActionResult SignIn()
        {
            return View();
        }

        [HttpGet]
        public IActionResult RequestPasswordReset()
        {
            return View();
        }

        [HttpGet]
        public IActionResult ResetPassword()
        {
            return View();
        }

        [HttpGet]
        public  async Task GoogleLogin()
        {
            await HttpContext.ChallengeAsync(GoogleDefaults.AuthenticationScheme,
                new AuthenticationProperties
                {
                    RedirectUri=Url.Action("GoogleResponse")
                });
        }

        [HttpGet]
        public  async Task<IActionResult> GoogleResponse()
        {
            // redirect to login page for Gmail.
            var result  = await HttpContext.AuthenticateAsync(GoogleDefaults.AuthenticationScheme);
//             var claims = result.Principal.Identities.First().Claims.Select(claim => new
//             {
//                 claim.Issuer,
//                 claim.OriginalIssuer,
//                 claim.Type,
//                 claim.Value
//             });

            if (result.Failure != null)
            {
                return BadRequest();
            }

            return RedirectToAction("Upload", "Upload");

        }

        /* POST */
        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid) 
            {
                var user = new ApplicationUser { UserName = model.Email, Email = model.Email };
                var result = await _userManager.CreateAsync(user, model.Password);

                if (result.Succeeded)
                {
                    await _signInManager.CanSignInAsync(user);
                    return RedirectToAction("Index", "Home");
                }

                foreach(var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }

            }

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> SignIn(SignInViewModel model)
        {
            if(ModelState.IsValid)
            {
                var user = await _userManager.FindByEmailAsync(model.Email);
                if(user != null)
                {
                    var result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, true, false);

                    if (result.Succeeded)
                    {
                        // later I will replace this with the upload page when I have it.
                        return RedirectToAction("Index", "Home");
                    }
                    else if (result.IsLockedOut) 
                    {
                        // lockout is turned off, but its ready to accept the error if enabled.
                        ModelState.AddModelError(string.Empty, "This account is locked, please try again later.");
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                    }
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                }
            }

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> RequestPasswordReset(PasswordResetViewModel model)
        {
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "User not found.");
                return View(model);
            }

            string resetToken = await _userManager.GeneratePasswordResetTokenAsync(user);
            var resetUrl = Url.Action("ResetPassword", "Account", new { token = resetToken, email = model.Email }, Request.Scheme);

            PasswordResetEmailViewModel resetModel = new PasswordResetEmailViewModel();
            resetModel.UserName = model.Email;
            resetModel.ResetUrl = resetUrl;
            string emailHtml = await _renderService.RenderToStringAsync("Account/PasswordResetEmail", resetModel);
            await _emailSender.SendEmailAsync(user.Email, "Password Reset From FileSharing", emailHtml);
            
            return View("PasswordRequestResetConfirmation");
        }
    }
}
