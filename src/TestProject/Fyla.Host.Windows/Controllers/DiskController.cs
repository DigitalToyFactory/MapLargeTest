using Fyla.FileSystem;
using Fyla.Orchestra;
using Microsoft.AspNetCore.Mvc;

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

        public DiskController(IMaestro maestro, IDiskRepository diskRepository)
        {
            _maestro = maestro;
            _diskRepository = diskRepository;
        }
    }
}

