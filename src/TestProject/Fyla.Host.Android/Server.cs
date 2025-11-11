using EmbedIO;
using EmbedIO.Routing;
using EmbedIO.WebApi;
using Newtonsoft.Json;
using System.Text;

namespace Fyla.Host.Android
{
    public static class Server
    {
        public static async Task StartAsync()
        {
            var url = "http://127.0.0.1:28111/";
            var server = new WebServer(o => o
                .WithUrlPrefix(url)
                .WithMode(HttpListenerMode.EmbedIO));

            server.WithWebApi("/api", m =>
            {
                m.WithController<HelloController>();
            });

            Console.WriteLine("Server starting on port 5000...");

            await server.RunAsync();
        }

        public class HelloController : WebApiController
        {
            [Route(HttpVerbs.Get, "/hello")]
            public async Task GetHello()
            {
                var json = JsonConvert.SerializeObject(new { ok = true, platform = "android" });
                await HttpContext.SendStringAsync(json, "application/json", Encoding.UTF8);
            }
        }                
    }
}

