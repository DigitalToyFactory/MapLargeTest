using System;
using LightInject;
using LightInject.Microsoft.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

namespace Fyla.Orchestra
{
    /// <summary>
    /// Forces ASP.NET Core to use an existing LightInject ServiceContainer.
    /// It captures the IServiceCollection in CreateBuilder (early in host startup)
    /// and builds the ServiceProvider from your container during CreateServiceProvider.
    /// </summary>
    public sealed class ExistingContainerFactory : IServiceProviderFactory<ServiceContainer>
    {
        private readonly ServiceContainer _container;
        private IServiceCollection _services = null!;

        public ExistingContainerFactory(ServiceContainer container)
        {
            _container = container ?? throw new ArgumentNullException(nameof(container));
        }

        // Called early: capture the IServiceCollection while returning your container as the "builder"
        public ServiceContainer CreateBuilder(IServiceCollection services)
        {
            _services = services ?? throw new ArgumentNullException(nameof(services));
            return _container;
        }

        // Called when the host builds the provider: populate into your container and return it
        public IServiceProvider CreateServiceProvider(ServiceContainer containerBuilder)
        {
            // extension from LightInject.Microsoft.DependencyInjection
            return containerBuilder.CreateServiceProvider(_services);
        }
    }
}
