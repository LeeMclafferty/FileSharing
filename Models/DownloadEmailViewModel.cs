namespace FileSharing.Models
{
    public class DownloadEmailViewModel
    {
        public string SendingEmail { get; set; } = string.Empty;
        public string RecipientEmail { get; set; } = string.Empty;
        public string DownloadUri { get; set; } = string.Empty;
        public IFormFile? File { get; set; }
    }
}
