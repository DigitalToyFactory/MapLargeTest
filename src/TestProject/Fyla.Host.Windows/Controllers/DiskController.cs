using Fyla.FileSystem;
using Fyla.Helpers;
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
        public IActionResult SearchDirectory([FromQuery] string path = "", [FromQuery] string pattern = "*.*", [FromQuery] bool deep = false)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(path))
                {
                    path = Directory.GetCurrentDirectory();
                }

                _maestro.Logger.Info($"Searching directory: {path}");

                var result = _diskRepository.Search(path, pattern, deep);

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

        [HttpPost]
        [Route("Upload")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> UploadFile([FromForm] FileUploadRequest request)
        {
            var file = request.File;
            var path = request.Path;

            if (file == null || file.Length == 0)
            {
                return BadRequest("No file uploaded.");
            }

            if (string.IsNullOrWhiteSpace(path))
            {
                return BadRequest("No path provided.");
            }

            var fileInfo = new FileInfo(path);
            var directory = fileInfo.Directory;

            if (directory == null || !directory.Exists)
            {
                return NotFound("Target directory not found.");
            }

            if (fileInfo.Exists)
            {
                return Conflict("File already exists.");
            }

            await using (var stream = new FileStream(fileInfo.FullName, FileMode.CreateNew))
            {
                await file.CopyToAsync(stream);
            }

            return Ok(new FileUploadResult
            {
                Checksum = fileInfo.CalculateFileMD5()
            });
        }


        public DiskController(IMaestro maestro, IDiskRepository diskRepository)
        {
            _maestro = maestro;
            _diskRepository = diskRepository;
        }
    }
}

