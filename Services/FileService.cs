using Azure.Storage.Blobs;
using NuGet.Protocol;
using System.Security.Cryptography;
using FileSharing.Interfaces;
using Azure.Storage;
using FileSharing.Models;
using Azure.Core;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Policy;
using Microsoft.AspNetCore.Identity.UI.Services;

namespace FileSharing.Services
{
    public class FileService : IFileService
    {
        private readonly string _storageAccount;
        private readonly string _blobContainerName;
        private readonly string _encryptionKey;
        private readonly string _encryptionIV;
        private readonly string _accountKey;
        private readonly IEmailSender _emailSender;
        private readonly BlobServiceClient _blobServiceClient;
        private readonly BlobContainerClient _filesContainer;
        private readonly IRenderService _renderService;
        

        public FileService(IConfiguration configuration, IEmailSender emailSender, IRenderService renderService)
        {
            _storageAccount = configuration["AzureBlobStorageAccount"] ?? throw new InvalidOperationException("Storage Account is not configured.");
            _blobContainerName = configuration["AzureBlobContainerName"] ?? throw new InvalidOperationException("Container Name is not configured.");
            _encryptionKey = configuration["UploadKey"] ?? throw new InvalidOperationException("Upload Key is not configured.");
            _encryptionIV = configuration["UploadIV"] ?? throw new InvalidOperationException("UploadIV is not configured.");
            _accountKey = configuration["AzureBlobAccountKey"] ?? throw new InvalidOperationException("Account Key is not configured.");

            var credential = new StorageSharedKeyCredential(_storageAccount, _accountKey);
            var blobUri = $"https://{_storageAccount}.blob.core.windows.net";
            _blobServiceClient = new BlobServiceClient(new Uri(blobUri), credential);
            _filesContainer = _blobServiceClient.GetBlobContainerClient(_blobContainerName);

            _emailSender = emailSender;
            _renderService = renderService;
        }

        public static MemoryStream EncryptStream(Stream inputStream, string key, string iv)
        {
            var outputStream = new MemoryStream();

            using (var aes = Aes.Create())
            {
                aes.Key = Convert.FromBase64String(key);
                aes.IV = Convert.FromBase64String(iv);

                using (var cryptoStream = new CryptoStream(outputStream, aes.CreateEncryptor(), CryptoStreamMode.Write))
                {
                    inputStream.CopyTo(cryptoStream);
                }
            }

            outputStream.Position = 0;
            return outputStream;
        }

        public async Task<List<BlobDto>> ListAsync()
        {
            List<BlobDto> files = new List<BlobDto>();

            await foreach (var file in _filesContainer.GetBlobsAsync())
            {
                string uri = _filesContainer.Uri.ToString();
                var name = file.Name;
                var fullUri = $"{uri}/{name}";

                files.Add(new BlobDto
                {
                    Uri = fullUri,
                    Name = name,
                    ContentType = file.Properties.ContentType
                });
            }

            return files;
        }

        public async Task<BlobResponseDto> UploadFileAsync(IFormFile blob)
        {
            BlobResponseDto response = new();
            BlobClient client = _filesContainer.GetBlobClient(blob.FileName);

            // Add encryption before upload

            await using (Stream? data = blob.OpenReadStream()) 
            {
                await client.UploadAsync(data, true);
            }

            // Add try catch here to actually catch errors and not set them to false.
            response.Status = $"File {blob.FileName} Uploaded Successfully.";
            response.Error = false;
            response.Blob.Uri = client.Uri.AbsoluteUri;
            response.Blob.Name = client.Name;

            return response;
        }

        public async Task<BlobDto?> DownloadAsync(string blobFileName)
        {
            BlobClient file = _filesContainer.GetBlobClient(blobFileName);

            if(await file.ExistsAsync())
            {
                var data = await file.OpenReadAsync();
                Stream blobContent = data;

                var content = await file.DownloadContentAsync();

                string name = blobFileName;
                string contentType = content.Value.Details.ContentType;

                return new BlobDto { Content = blobContent, Name = name, ContentType = contentType };
            }

            return null;
        }

        public async Task<BlobResponseDto> DeleteAsync(string blobFilName)
        {
            BlobClient file = _filesContainer.GetBlobClient(blobFilName);

            if(await file.ExistsAsync())
            {
                await file.DeleteAsync();
                return new BlobResponseDto { Error = false, Status = $"File: {blobFilName} has been deleted." };
            }

            return new BlobResponseDto { Error = true, Status = $"File: {blobFilName} was not found and could not be deleted." };
        }

        // for testing connection
        public async Task ListBlobContainersAsync()
        {
            var containers = _blobServiceClient.GetBlobContainersAsync();

            await foreach (var container in containers)
            {
                Console.WriteLine(container.Name);
            }
        }

        public async Task<string> SendDownloadEmailAsync(DownloadEmailViewModel model)
        {
            try
            {
                string emailHtml = await _renderService.RenderToStringAsync("Download/DownloadEmail", model);
                await _emailSender.SendEmailAsync(
                    model.RecipientEmail,
                    $"New File Received from {model.SendingEmail}",
                    emailHtml);

                // Return success response
                return "Message Sent Successfully";
            }
            catch (Exception ex)
            {
                // Return error response
                return $"An error occurred while sending the email: ${ex.Message}";
            }
        }
    }
}
