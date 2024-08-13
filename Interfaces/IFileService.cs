using FileSharing.Models;
using Microsoft.AspNetCore.Mvc;

namespace FileSharing.Interfaces
{
    public interface IFileService
    {
        Task<List<BlobDto>> ListAsync();
        Task<BlobResponseDto> UploadFileAsync(IFormFile File);
        Task<BlobDto?> DownloadAsync(string blobFileName);
        Task<BlobResponseDto> DeleteAsync(string blobFileName);
        Task<string> SendDownloadEmailAsync(DownloadEmailViewModel model);
    }
}
