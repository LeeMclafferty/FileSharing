namespace FileSharing.Interfaces
{
    public interface IRenderService
    {
        Task<string> RenderToStringAsync(string viewName, object model);
    }
}
