using Microsoft.AspNetCore.Mvc;

namespace FileSharing.Controllers
{
    public class UploadController : Controller
    {
        public IActionResult Upload()
        {
            return View();
        }
    }
}
