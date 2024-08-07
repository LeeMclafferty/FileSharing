using FileSharing.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.IO;
using System.Threading.Tasks;
using FileSharing.Interfaces;
using Microsoft.Extensions.Logging;

namespace FileSharing.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FilesApiController : ControllerBase
    {
        private readonly IFileService _fileService;
        private readonly long _maxFileSize = 10 * 1024 * 1024; // 10 MB
        private readonly string[] _permittedExtensions = { ".jpg", ".jpeg", ".png", ".pdf", ".doc", ".docx", ".xlm", ".txt" };

        public FilesApiController(IFileService fileService)
        {
            _fileService = fileService;
        }

        [HttpGet]
        public async Task<IActionResult> ListAllBlobs()
        {
            var result = await _fileService.ListAsync();
            return Ok(result);
        }

        [HttpPost("file")]
        public async Task<IActionResult> UploadFile([FromForm] IFormFile file)
        {
            var result = _fileService.UploadFileAsync(file);
            return Ok(result);
        }

        [HttpGet]
        [Route("filename")]
        public async Task<IActionResult> Download(string filename)
        {
            var result = await _fileService.DownloadAsync(filename);
            return File(result.Content, result.ContentType, result.Name);
        }

        [HttpDelete]
        [Route("filename")]
        public async Task<IActionResult> Delete(string filename) 
        {
            var result = await _fileService.DeleteAsync(filename);
            return Ok(result);
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
