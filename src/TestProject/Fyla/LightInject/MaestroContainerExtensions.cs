using Fyla.Orchestra;
using LightInject;

namespace Fyla.LightInject
{
    public static class MaestroContainerExtensions
    {
        //NOTE: hack to fix issues with using LightInject.Microsoft.DependencyInjection and LightInjectServiceProviderFactory on ASP.NET CORE 9
        public static void ReplaceContainer(this MaestroBase maestro, ServiceContainer adopted)
        {
            if (adopted == null || ReferenceEquals(adopted, maestro.Container))
            {
                return;
            }

            foreach (var reg in maestro.Container.AvailableServices)
            {
                // avoid duplicates
                bool exists = adopted.AvailableServices.Any(s =>
                    s.ServiceType == reg.ServiceType &&
                    string.Equals(s.ServiceName, reg.ServiceName, StringComparison.Ordinal));

                if (exists)
                {
                    continue;
                }

                try
                {
                    //full registration with factory or implementing type
                    if (reg.FactoryExpression != null || reg.ImplementingType != null)
                    {
                        adopted.Register(reg);
                        continue;
                    }

                    //instance registration
                    if (reg.Value != null)
                    {
                        adopted.RegisterInstance(reg.ServiceType, reg.Value, reg.ServiceName);
                        continue;
                    }

                    //skip anything malformed
                    maestro.Logger.Warn($"Skipped registration for {reg.ServiceType.FullName}: no factory, instance, or implementing type.");
                }
                catch (Exception ex)
                {
                    maestro.Logger.Warn(ex, $"Failed to adopt service {reg.ServiceType.FullName} ({reg.ServiceName ?? "default"}).");
                }
            }

            //update pointer to the adopted container
            maestro.Container = adopted;

            //ensure Maestro itself is visible in the container
            adopted.RegisterInstance<IMaestro>(maestro);
        }
    }
}
