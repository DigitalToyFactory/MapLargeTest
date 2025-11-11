using LightInject;

namespace Fyla.Orchestra
{
    public static class MaestroExtensions
    {
        public static T Service<T>(this IMaestro maestro)
        {
            var result = maestro.Services.GetInstance<T>();
            return result;
        }

        public static IServiceRegistry RegisterFrom<TCompositionRoot>(this IMaestro maestro) where TCompositionRoot : ICompositionRoot, new()
        {
            var container = maestro.Container as ServiceContainer;

            container?.RegisterFrom<TCompositionRoot>();

            return maestro.Container;
        }

        public static IServiceRegistry RegisterFrom(this IMaestro maestro, ICompositionRoot root)
        {
            var container = maestro.Container as ServiceContainer;

            container?.RegisterFrom(root);

            return maestro.Container;
        }

        public static void Register<T, TService>(this IServiceRegistry registry, Func<IServiceFactory, T, TService> factory)
        {
            registry.Register(new ServiceRegistration
            {
                ServiceType = typeof(TService),
                FactoryExpression = factory,
                Lifetime = new PerRequestLifeTime()
            });
        }
    }
}
