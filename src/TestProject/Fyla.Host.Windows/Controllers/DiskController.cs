using Fyla.FileSystem;
using Fyla.Orchestra;
using Fyla.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;

namespace Fyla.Host.Windows.Controllers
{
    [ApiController]
    [Route("Disk")]
    public class DiskController : ControllerBase
    {
        private readonly IMaestro _maestro;
        private readonly IDiskRepository _diskRepository;

        [HttpGet("List")]
        public IActionResult ListDirectory([FromQuery] string path = "")
        {
            try
            {
                if (string.IsNullOrWhiteSpace(path))
                {
                    path = Directory.GetCurrentDirectory();
                }

                _maestro.Logger.Info($"Listing directory: {path}");

                var result = _diskRepository.GetDirectoryContents(path);

                return Ok(result);
            }
            catch (Exception ex)
            {
                _maestro.Logger.Error(ex, $"Failed to list directory: {path}");
                return BadRequest(new
                {
                    Error = ex.Message,
                    Path = path
                });
            }
        }

        [HttpGet("Search")]
        public IActionResult ListDirectory([FromQuery] string path = "", [FromQuery] string pattern = "*.*")
        {
            try
            {
                if (string.IsNullOrWhiteSpace(path))
                {
                    path = Directory.GetCurrentDirectory();
                }

                _maestro.Logger.Info($"Searching directory: {path}");

                var result = _diskRepository.Search(path, pattern, true);

                return Ok(result);
            }
            catch (Exception ex)
            {
                _maestro.Logger.Error(ex, $"Failed to search directory: {path}");
                return BadRequest(new
                {
                    Error = ex.Message,
                    Path = path
                });
            }
        }

        [HttpGet]
        [Route("Download")]
        public IActionResult DownloadFile([FromQuery] FileDownloadRequest request)
        {
            var fileInfo = new FileInfo(request.Path);

            if (!fileInfo.Exists)
            {
                return NotFound();
            }

            var provider = new FileExtensionContentTypeProvider();
            if (!provider.TryGetContentType(fileInfo.FullName, out var contentType))
            {
                contentType = "application/octet-stream"; // fallback for unknown types
            }

            return File(fileInfo.OpenRead(), contentType, fileInfo.Name);
        }

        public DiskController(IMaestro maestro, IDiskRepository diskRepository)
        {
            _maestro = maestro;
            _diskRepository = diskRepository;
        }
    }
}

