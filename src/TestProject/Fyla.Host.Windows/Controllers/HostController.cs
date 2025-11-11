using Fyla.Orchestra;
using Microsoft.AspNetCore.Mvc;

namespace Fyla.Host.Windows.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class HostController : ControllerBase
    {
        private readonly IMaestro _maestro;

        [HttpGet("Terminate")]
        public IActionResult Terminate()
        {
            _maestro.Logger.Info("Termination requested via HostController.");

            _maestro.Terminate();

            return Ok(new
            {
                Message = "Termination signal sent.",
                Timestamp = DateTime.UtcNow
            });
        }

        public HostController(IMaestro maestro)
        {
            _maestro = maestro;
        }
    }
}

