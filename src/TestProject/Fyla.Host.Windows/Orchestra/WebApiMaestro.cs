using Firefly.Settings;
using Fyla.FileSystem;
using LightInject;
using LightInject.Microsoft.DependencyInjection;
using Microsoft.AspNetCore.Builder;
using Fyla.LightInject;

namespace Fyla.Orchestra
{
    public sealed class WebApiMaestro : MaestroBase
    {
        public bool Initialized { get; private set; }

        protected override void OnInitServices(IServiceRegistry registry)
        {
            if (Initialized)
            {
                return;
            }

            //registry.Register<ISettingsManager, SettingsManager>();
            registry.Register<IDiskRepository, WindowsDiskRepository>();

            Initialized = true;
        }

        public override async Task RunAsync(string[] args)
        {
            Logger.Info("WebApi starting.");

            var builder = WebApplication.CreateBuilder(args);

            builder.Host.UseServiceProviderFactory(new LightInjectServiceProviderFactory());
            builder.Host.ConfigureContainer<IServiceContainer>(container =>
            {
                this.ReplaceContainer((ServiceContainer)container);
            });

            //builder.Services.AddSingleton<IMaestro>(this);
            builder.Services.AddControllers();
            builder.Services.AddOpenApi();

            var app = builder.Build();

            app.Lifetime.ApplicationStarted.Register(Ready);

            if (app.Environment.IsDevelopment())
            {
                app.MapOpenApi();
                app.UseSwaggerUI(c =>
                {
                    c.SwaggerEndpoint("/openapi/v1.json", "MapLarge Fyla API v1");
                });
            }

            app.UseHttpsRedirection();
            app.MapControllers();

            Logger.Info("Kestrel RunAsync starting.");

            try
            {
                await app.RunAsync(CancelOnTerminate.Token);
            }
            catch (TaskCanceledException)
            {
                // graceful shutdown via CancelOnTerminate
                Logger.Info("WebApi cancelled by termination token.");
            }
        }
    }
}
