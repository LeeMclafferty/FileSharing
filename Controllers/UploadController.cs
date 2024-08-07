using FileSharing.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.Threading.Tasks;

namespace FileSharing.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UploadController : Controller
    {
        private readonly FileUploadService _fileUploadService;
        private readonly long _maxFileSize = 10 * 1024 * 1024; // 10 MB
        private readonly string[] _permittedExtensions = { ".jpg", ".jpeg", ".png", ".pdf", ".doc", ".docx", ".xlm", ".txt" };

        public UploadController(FileUploadService fileUploadService)
        {
            _fileUploadService = fileUploadService;
        }

        [HttpPost]
        public async Task<IActionResult> Upload([FromBody] IFormFile file)
        {
            if(file == null)
            {
                return BadRequest("File is not selected or is empty");
            }

            string validationError = ValidateFile(file);
            if(!validationError.IsNullOrEmpty())
            {
                return BadRequest(validationError);
            }

            var fileUrl = await _fileUploadService.UploadFileAsync(file);
            return Ok(new { fileUrl });
        }

        private string ValidateFile(IFormFile file)
        {
            // Validate file size
            if (file.Length > _maxFileSize)
            {
                return $"File size exceeds {_maxFileSize / (1024 * 1024)} MB limit.";
            }

            // Validate file type
            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (string.IsNullOrEmpty(ext) || Array.IndexOf(_permittedExtensions, ext) < 0)
            {
                return "Invalid file type.";
            }

            return string.Empty;
        }
    }

}
