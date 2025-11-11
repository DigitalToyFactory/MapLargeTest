using System.Reflection;
using LightInject;
using NLog;

namespace Fyla.Orchestra
{
    public abstract class MaestroBase : IMaestro
    {
        private static readonly ILogger _logger = LogManager.GetCurrentClassLogger();

        private ServiceContainer _services;
        private readonly ManualResetEvent _ready;
        private readonly ManualResetEvent _terminateRequested;
        private readonly ManualResetEvent _terminateReady;
        private readonly ManualResetEvent _terminated;
        private readonly CancellationTokenSource _cancelOnTerminate;

        protected Task? _runTask;
        protected Task? _loopTask;

        public IServiceFactory Services => _services;
        public IServiceRegistry Registry => _services;
        public IServiceContainer Container
        {
            get => _services;
            set
            {
                if(value is ServiceContainer container)
                {
                    _services = container;
                }
            }
        }

        public ManualResetEvent ReadyEvent => _ready;
        public ManualResetEvent TerminateReadyEvent => _terminateReady;
        public ManualResetEvent TerminatedEvent => _terminated;
        public CancellationTokenSource CancelOnTerminate => _cancelOnTerminate;

        protected virtual int LoopDelayMs { get; } = 250;

        public ILogger Logger => _logger;

        public void Initialize(string[] args)
        {
            var fileInfo = new FileInfo(Assembly.GetEntryAssembly()?.Location ?? Assembly.GetExecutingAssembly().Location);

            SafeLog(() =>
            {
                _logger.Info("================================================");
                _logger.Info("Initializing application environment...");
                _logger.Info($"{fileInfo.Name}, {fileInfo.Length} bytes, created {fileInfo.CreationTimeUtc} (UTC)");
                _logger.Info($"Args: {(args.Any() ? string.Join(" ", args) : "[none]")}");
                _logger.Info("================================================");
            });

            OnInitServices(_services);
        }

        protected virtual void OnInitServices(IServiceRegistry registry) { }

        public abstract Task RunAsync(string[] args);

        protected virtual Task LoopTickAsync() => Task.CompletedTask;

        public virtual async Task LoopAsync()
        {
            try
            {
                while (!_terminateRequested.WaitOne(0) && !_cancelOnTerminate.IsCancellationRequested)
                {
                    try
                    {
                        await LoopTickAsync();
                    }
                    catch (Exception ex)
                    {
                        SafeLog(() => _logger.Error(ex, "Error during Maestro loop tick."));
                    }

                    try
                    {
                        await Task.Delay(LoopDelayMs, _cancelOnTerminate.Token);
                    }
                    catch (TaskCanceledException)
                    {
                        break; // graceful shutdown
                    }
                }

                SafeLog(() => _logger.Info("Termination requested, waiting for active tasks to complete..."));

                var pending = new List<Task>();
                if (_runTask != null && !_runTask.IsCompleted)
                {
                    pending.Add(_runTask);
                }

                if (pending.Count > 0)
                {
                    await Task.WhenAll(pending);
                }

                SafeLog(() => _logger.Info("All pending tasks completed, finalizing termination..."));
            }
            catch (Exception ex)
            {
                SafeLog(() => _logger.Error(ex, "Exception during Maestro loop termination."));
            }
            finally
            {
                _terminated.Set();
                SafeLog(() => _logger.Info("Application marked Terminated (final)."));
            }
        }

        public void Ready()
        {
            if (!_ready.WaitOne(0))
            {
                _ready.Set();
                SafeLog(() => _logger.Info("Application marked Ready."));
            }
        }

        public async Task WaitForReadyAsync()
        {
            while (!_ready.WaitOne(0))
            {
                await Task.Delay(25);
            }
        }

        public void Terminate()
        {
            if (!_terminateRequested.WaitOne(0))
            {
                SafeLog(() => _logger.Info("Terminate() invoked; signaling shutdown sequence..."));
                Task.Run(OnTerminationAsync);
            }
        }

        protected virtual Task OnTerminationAsync()
        {
            if (!_terminateRequested.WaitOne(0))
            {
                _terminateRequested.Set();
                _terminateReady.Set();
                _cancelOnTerminate.Cancel();
                SafeLog(() => _logger.Info("Termination sequence requested."));
            }

            return Task.CompletedTask;
        }

        public async Task StartAsync(string[] args)
        {
            Initialize(args);

            //application context
            _runTask = RunAsync(args);            
            await WaitForReadyAsync();

            //maestro loop
            _loopTask = LoopAsync();
        }

        public static async Task<T> Run<T>(params string[] args) where T : MaestroBase, new()
        {
            var maestro = new T();
            await maestro.StartAsync(args);
            return maestro;
        }

        protected void SafeLog(Action logAction)
        {
            try 
            { 
                logAction?.Invoke(); 
            }
            catch { }
        }

        public Task Completed => Task.WhenAll(_runTask, _loopTask);

        protected MaestroBase()
        {
            var options = new ContainerOptions
            {
                EnablePropertyInjection = false,
                EnableCurrentScope = true
            };

            _services = new ServiceContainer(options);
            _services.SetDefaultLifetime<PerContainerLifetime>();
            _services.RegisterInstance<IMaestro>(this);
            _services.RegisterInstance(Container);

            _ready = new ManualResetEvent(false);
            _terminateRequested = new ManualResetEvent(false);
            _terminateReady = new ManualResetEvent(false);
            _terminated = new ManualResetEvent(false);
            _cancelOnTerminate = new CancellationTokenSource();
        }
    }
}