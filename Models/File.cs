namespace FileSharing.Models
{
    public class File
    {
        public int Id { get; set; }
        public string FileName { get; set; } = string.Empty;
        public string FilePath { get; set; } = string.Empty;
        public int UploadedBy { get; set; }
        public DateTime UploadDate { get; set; }
    }
}
