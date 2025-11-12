using Fyla.FileSystem;
using Fyla.Helpers;
using Fyla.Orchestra;
using Fyla.Services;
using Fyla.Settings;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;
using System.IO;
using System.Runtime.InteropServices.JavaScript;

namespace Fyla.Host.Windows.Controllers
{
    [ApiController]
    [Route("Disk")]
    public class DiskController : ControllerBase
    {
        private readonly IMaestro _maestro;
        private readonly IDiskRepository _diskRepository;
        private readonly ISettingsManager _settings;

        private List<string> _allowedRoots = new List<string>();

        public List<string> AllowedRoots
        {
            get => _allowedRoots;
            set => _allowedRoots = value;
        }
        
        public bool IsAuthorized(string path)
        {
            var fullPath = Path.GetFullPath(path).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar) + Path.DirectorySeparatorChar;

            foreach (var root in _allowedRoots)
            {
                var fullRoot = Path.GetFullPath(root).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar) + Path.DirectorySeparatorChar;
                if (fullPath.StartsWith(fullRoot, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }


        [HttpGet("List")]
        public IActionResult ListDirectory([FromQuery] string path = "")
        {
            try
            {
                if (string.IsNullOrWhiteSpace(path))
                {
                    path = Directory.GetCurrentDirectory();
                }

                if (!IsAuthorized(path))
                {
                    return Forbid();
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

                if (!IsAuthorized(path))
                {
                    return Forbid();
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

            if (!IsAuthorized(fileInfo.FullName))
            {
                return Forbid();
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

            if (!IsAuthorized(path))
            {
                return Forbid();
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

        [HttpDelete]
        [Route("Delete")]
        public IActionResult DeleteItem([FromQuery] string path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                return BadRequest("No path provided.");
            }

            if (!IsAuthorized(path))
            {
                return Forbid();
            }

            try
            {
                _diskRepository.Delete(path);
                return Ok();
            }
            catch (Exception ex)
            {
                return Problem(ex.Message);
            }
        }

        [HttpPost]
        [Route("Copy")]
        public IActionResult CopyItem([FromQuery] string src, [FromQuery] string dest)
        {
            if (string.IsNullOrWhiteSpace(src) || string.IsNullOrWhiteSpace(dest))
            {
                return BadRequest("Source or destination missing.");
            }

            if (!IsAuthorized(src) || !IsAuthorized(dest))
            {
                return Forbid();
            }

            try
            {
                _diskRepository.Copy(src, dest);
                return Ok();
            }
            catch (Exception ex)
            {
                return Problem(ex.Message);
            }
        }

        [HttpPost]
        [Route("Move")]
        public IActionResult MoveItem([FromQuery] string src, [FromQuery] string dest)
        {
            if (string.IsNullOrWhiteSpace(src) || string.IsNullOrWhiteSpace(dest))
            {
                return BadRequest("Source or destination missing.");
            }

            if (!IsAuthorized(src) || !IsAuthorized(dest))
            {
                return Forbid();
            }

            try
            {
                _diskRepository.Move(src, dest);
                return Ok();
            }
            catch (Exception ex)
            {
                return Problem(ex.Message);
            }
        }

        private static void CopyDirectory(string sourceDir, string destDir)
        {
            var dir = new DirectoryInfo(sourceDir);
            if (!dir.Exists)
                throw new DirectoryNotFoundException($"Source directory not found: {sourceDir}");

            Directory.CreateDirectory(destDir);

            foreach (var file in dir.GetFiles())
            {
                string targetFilePath = Path.Combine(destDir, file.Name);
                file.CopyTo(targetFilePath, overwrite: false);
            }

            foreach (var subDir in dir.GetDirectories())
            {
                string newDestDir = Path.Combine(destDir, subDir.Name);
                CopyDirectory(subDir.FullName, newDestDir);
            }
        }

        private void RegisterSettings()
        {
            var defaultAllowedRoots =  new List<string>() { "C:\\", "X:\\", "Y:\\" };
            var test = defaultAllowedRoots.Serialize();

            AllowedRoots = _settings.GetSetDefaultValue("DiskController.AllowedRoots", defaultAllowedRoots.Serialize()).Deserialize<List<string>>();
        }

        public DiskController(IMaestro maestro, IDiskRepository diskRepository, ISettingsManager settings)
        {
            _maestro = maestro;
            _diskRepository = diskRepository;
            _settings = settings;

            RegisterSettings();
        }
    }
}

