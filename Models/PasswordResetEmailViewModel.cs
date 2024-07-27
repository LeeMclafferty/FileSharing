namespace FileSharing.Models
{
    public class PasswordResetEmailViewModel
    {
        public string UserName { get; set; } = string.Empty;
        public string ResetUrl { get; set; } = string.Empty;
    }
}
