using Microsoft.AspNetCore.Http;

namespace Fyla.Services
{
    public class FileUploadRequest
    {
        public IFormFile? File { get; set; }
        public string? Path { get; set; }
    }
}
