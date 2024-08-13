using FileSharing.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace FileSharing.Controllers
{
    public class UploadController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public UploadController(UserManager<ApplicationUser> userManager) 
        {
            _userManager = userManager;
        }

        public async Task<IActionResult> Upload(string userEmail)
        {
            var user = await _userManager.FindByEmailAsync(userEmail);
            return View(user);
        }

        public IActionResult UploadSuccessful()
        {
            return View();
        }
    }
}
