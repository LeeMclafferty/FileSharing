using FileSharing.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace FileSharing.Controllers
{
    public class UploadController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public UploadController(UserManager<ApplicationUser> userManager) 
        {
            _userManager = userManager;
        }

        public IActionResult Upload()
        {
            return View();
        }

        public IActionResult UploadSuccessful()
        {
            return View();
        }
    }
}
