using System.Net.Http;
using System.Diagnostics;

namespace Fyla.Host.Android
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            var window = new Window(new AppShell());

            _ = Task.Run(async () =>
            {
                await Task.Delay(1000 * 20);

                try
                {
                    using var tcp = new System.Net.Sockets.TcpClient();
                    await tcp.ConnectAsync("127.0.0.1", 28111);
                    System.Diagnostics.Debug.WriteLine("TCP CONNECT: OK");
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine("TCP CONNECT failed: " + ex.Message);
                }

                try
                {
                    var handler = new SocketsHttpHandler { AllowAutoRedirect = false };
                    using var http = new HttpClient(handler);
                    var req = new HttpRequestMessage(HttpMethod.Get, "http://127.0.0.1:28111/api/hello") { Version = new Version(1,1) };
                    var res = await http.SendAsync(req);
                    System.Diagnostics.Debug.WriteLine("HELLO STATUS: " + (int)res.StatusCode);
                    System.Diagnostics.Debug.WriteLine("HELLO BODY: " + await res.Content.ReadAsStringAsync());

                }
                catch (Exception ex)
                {
                    Debug.WriteLine("HTTP test failed: " + ex.Message);
                }
            });

            return window;
        }
    }
}