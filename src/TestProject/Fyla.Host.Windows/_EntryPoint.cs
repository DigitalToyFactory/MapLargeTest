using Fyla.Orchestra;

namespace Fyla.Host.Windows
{
    public static class EntryPoint
    {
        public static async Task Main(string[] args)
        {
            var maestro = await MaestroBase.Run<WebApiMaestro>(args);
            await maestro.Completed;
        }
    }
}
