using System.Threading;
using System.Threading.Tasks;
using LightInject;
using NLog;

namespace Fyla.Orchestra
{
    public interface IMaestro
    {
        IServiceFactory Services { get; }
        IServiceRegistry Registry { get; }
        IServiceContainer Container { get; }

        ManualResetEvent ReadyEvent { get; }
        ManualResetEvent TerminateReadyEvent { get; }
        ManualResetEvent TerminatedEvent { get; }
        CancellationTokenSource CancelOnTerminate { get; }

        ILogger Logger { get; }

        Task RunAsync(string[] args);
        Task LoopAsync();
        void Ready();
        Task WaitForReadyAsync();
        void Terminate();  
    }
}

