using FileSharing.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.IO;
using System.Threading.Tasks;
using FileSharing.Interfaces;
using Microsoft.Extensions.Logging;
using FileSharing.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Castle.Core.Smtp;

namespace FileSharing.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FilesController : Controller
    {
        private readonly IFileService _fileService;
        private readonly IConfiguration? _config;

        public FilesController(IFileService fileService, IConfiguration configuration)
        {
            _fileService = fileService;
            _config = configuration;
        }

        [HttpGet]
        public IActionResult DownloadEmail(DownloadEmailViewModel model)
        {
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> ListAllBlobs()
        {
            var result = await _fileService.ListAsync();
            return Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> UploadFile(DownloadEmailViewModel model)
        {
            if (model == null || model.File == null || _config == null)
                return BadRequest();

            var result = await _fileService.UploadFileAsync(model.File);

            string? blobUri = _config["AzureBlobStorage:BlobUri"];
            string fileName = model.File.FileName;
            string? sasToken = _config["AzureBlobStorage:SasToken"];
            model.DownloadUri = blobUri + fileName + sasToken;

            await SendDownloadEmail(model);

            return RedirectToAction("UploadSuccessful", "Upload");
        }

        [HttpGet]
        [Route("download")]
        public async Task<IActionResult> Download(string filename)
        {
            var result = await _fileService.DownloadAsync(filename);

            if (result == null || result.Content == null || result.ContentType == null)    
                return BadRequest();

            return File(result.Content, result.ContentType, result.Name);
        }

        [HttpDelete]
        [Route("delete")]
        public async Task<IActionResult> Delete(string filename) 
        {
            var result = await _fileService.DeleteAsync(filename);
            return Ok(result);
        }

        [HttpPost("send")]
        public async Task<IActionResult> SendDownloadEmail(DownloadEmailViewModel model)
        {
            if (ModelState.IsValid)
            {
                var result = await _fileService.SendDownloadEmailAsync(model);
                Console.Write(result);
                return Ok(result);
            }
            return BadRequest("Invalid data");
        }

    }
}
