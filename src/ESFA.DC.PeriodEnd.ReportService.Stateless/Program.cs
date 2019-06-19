using System;
using System.Diagnostics;
using System.Threading;
using Autofac;
using Autofac.Integration.ServiceFabric;
using ESFA.DC.PeriodEnd.ReportService.Service;
using ESFA.DC.ServiceFabric.Common.Config;
using ESFA.DC.ServiceFabric.Common.Config.Interface;
using Microsoft.ServiceFabric.Services.Runtime;

namespace ESFA.DC.PeriodEnd.ReportService.Stateless
{
    public static class Program
    {
        /// <summary>
        /// This is the entry point of the service host process.
        /// </summary>
        public static void Main()
        {
            try
            {
                IServiceFabricConfigurationService serviceFabricConfigurationService = new ServiceFabricConfigurationService();

                // Setup Autofac
                ContainerBuilder builder = DIComposition.BuildContainer(serviceFabricConfigurationService);

                // Register the Autofac magic for Service Fabric support.
                builder.RegisterServiceFabricSupport();

                // Register the stateless service.
                builder.RegisterStatelessService<ServiceFabric.Common.Stateless>("ESFA.DC.PeriodEnd1920.ReportService.StatelessType");

                using (var container = builder.Build())
                {
                    container.Resolve<EntryPoint>();

                    ServiceEventSource.Current.ServiceTypeRegistered(Process.GetCurrentProcess().Id, typeof(ServiceFabric.Common.Stateless).Name);

                    // Prevents this host process from terminating so services keep running.
                    Thread.Sleep(Timeout.Infinite);
                }
            }
            catch (Exception e)
            {
                ServiceEventSource.Current.ServiceHostInitializationFailed(e + Environment.NewLine + (e.InnerException?.ToString() ?? "No inner exception"));
                throw;
            }
        }
    }
}
