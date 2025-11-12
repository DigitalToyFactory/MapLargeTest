using Fyla.Settings;
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

            registry.Register<ISettingsManager, SettingsManager>();
            registry.Register<IDiskRepository, WindowsDiskRepository>();

            var settingsManager = this.Service<ISettingsManager>();
            settingsManager.Initialize();

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
            
            //NOTE: OpenApi not playing nice with Swagger, file browse not showing in API UI
            //builder.Services.AddOpenApi();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
                {
                    Title = "MapLarge Fyla API",
                    Version = "v1"
                });
            });

            builder.Services.AddCors(o =>
            {
                o.AddPolicy("any", p => p.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod());
            });

            var app = builder.Build();

            app.Lifetime.ApplicationStarted.Register(Ready);

            if (app.Environment.IsDevelopment())
            {
                //app.MapOpenApi();
                app.UseSwagger();
                app.UseSwaggerUI(c =>
                {
                    //c.SwaggerEndpoint("/openapi/v1.json", "MapLarge Fyla API v1");
                    c.SwaggerEndpoint("/swagger/v1/swagger.json", "MapLarge Fyla API v1");
                });
            }

            app.UseCors("any");
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
