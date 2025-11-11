namespace Fyla.Services
{
    public interface IFileManager
    {
        Task<byte[]> GetFileAsync(FileDownloadRequest request);
        Task<FileUploadResult> SaveFileAsync(FileUploadRequest request);
    }
}
