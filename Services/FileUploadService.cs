using Azure.Storage.Blobs;
using NuGet.Protocol;
using System.Security.Cryptography;
using FileSharing.Interfaces;

namespace FileSharing.Services
{
    public class FileUploadService : IFileUploadService
    {
        private readonly string _blobConnectionString;
        private readonly string _blobContainerName;
        private readonly string _encryptionKey;
        private readonly string _encryptionIV;

        public FileUploadService(IConfiguration configuration)
        {
            _blobConnectionString = configuration["AzureBlobStorage:ConnectionString"];
            _blobContainerName = configuration["AzureBlobStorage:ContainerName"];
            _encryptionKey = configuration["UploadKey"];
            _encryptionIV = configuration["UploadIV"];
        }

        public async Task<string> UploadFileAsync(IFormFile file)
        {
            var blobServiceClient = new BlobServiceClient(_blobConnectionString);
            var blobContainerClient = blobServiceClient.GetBlobContainerClient(_blobContainerName);
            var blobClient = blobContainerClient.GetBlobClient(Guid.NewGuid().ToString());

            using(var stream = new MemoryStream())
            {
                await file.CopyToAsync(stream);
                stream.Position = 0;

                var encryptedStream = EncryptStream(stream, _encryptionKey, _encryptionIV);

                await blobClient.UploadAsync(stream, true);
            }

            return blobClient.Uri.ToString();
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
    }
}
