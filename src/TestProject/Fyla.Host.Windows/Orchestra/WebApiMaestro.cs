using Microsoft.AspNetCore.Builder;

namespace Fyla.Orchestra
{
    public sealed class WebApiMaestro : MaestroBase
    {
        public override async Task RunAsync(string[] args)
        {
            Logger.Info("WebApi starting.");

            var builder = WebApplication.CreateBuilder(args);

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
