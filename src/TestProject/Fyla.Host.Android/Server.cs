using EmbedIO;
using EmbedIO.Routing;
using EmbedIO.WebApi;
using Newtonsoft.Json;
using System.Text;
using System.Threading.Tasks;
using WatsonWebserver;
using WatsonWebserver.Core;

using HttpMethod = WatsonWebserver.Core.HttpMethod;

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

        static WebserverBase? _server;
        static int _started = 0;

        [Obsolete("Used when debugging EmbedIO")]
        public static Task StartAsyncWatson()
        {
            if (System.Threading.Interlocked.Exchange(ref _started, 1) == 1)
                return Task.CompletedTask;

            return Task.Run(() =>
            {
                System.Diagnostics.Debug.WriteLine("");

                try
                {
                    var settings = new WebserverSettings("127.0.0.1", 28111);
                    _server = new Webserver(settings, DefaultRoute);

                    _server.Routes.PreAuthentication.Static.Add(
                        WatsonWebserver.Core.HttpMethod.GET, "/api/hello",
                        async ctx =>
                        {
                            var json = Newtonsoft.Json.JsonConvert.SerializeObject(new { ok = true, platform = "android" });
                            ctx.Response.StatusCode = 200;
                            ctx.Response.ContentType = "application/json";
                            await ctx.Response.Send(json);
                        });

                    System.Diagnostics.Debug.WriteLine("Server: starting 127.0.0.1:28111");
                    _server.Start(); // blocking
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine("Server: start failed: " + ex);
                }

                System.Diagnostics.Debug.WriteLine("");
            });
        }

        [Obsolete("Used when debugging EmbedIO")]
        static async Task DefaultRoute(HttpContextBase ctx) => await ctx.Response.Send("OK");

        [Obsolete("Used this for debugging when EmbedIO and Watson repeatedly failed")]
        public static Task StartAsyncWorking()
        {
            return Task.Run(async () =>
            {
                var listener = new System.Net.Sockets.TcpListener(System.Net.IPAddress.Loopback, 28111);
                listener.Start();
                System.Diagnostics.Debug.WriteLine("TcpListener: 127.0.0.1:28111");

                while (true)
                {
                    var client = await listener.AcceptTcpClientAsync();
                    _ = Task.Run(async () =>
                    {
                        using var c = client;
                        using var s = c.GetStream();

                        // minimal HTTP/1.1 response
                        var body = "{\"ok\":true,\"platform\":\"android\"}";
                        var resp =
                            "HTTP/1.1 200 OK\r\n" +
                            "Content-Type: application/json\r\n" +
                            $"Content-Length: {System.Text.Encoding.UTF8.GetByteCount(body)}\r\n" +
                            "Connection: close\r\n\r\n" +
                            body;

                        var bytes = System.Text.Encoding.UTF8.GetBytes(resp);
                        await s.WriteAsync(bytes, 0, bytes.Length);
                        await s.FlushAsync();
                    });
                }
            });
        }        
    }
}

