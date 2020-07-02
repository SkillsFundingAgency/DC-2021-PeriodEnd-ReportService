using Autofac;
using ESFA.DC.PeriodEnd.ReportService.Reports.Interface;

namespace ESFA.DC.PeriodEnd.ReportService.Modules
{
    public abstract class AbstractReportModule<T> : Module where T : IReport
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<T>().As<IReport>();

            RegisterDataProviders(builder);

            RegisterModelBuilder(builder);

            RegisterRenderService(builder);

            RegisterServices(builder);

            RegisterPersistenceService(builder);
        }

        protected abstract void RegisterDataProviders(ContainerBuilder builder);

        protected abstract void RegisterModelBuilder(ContainerBuilder builder);

        protected abstract void RegisterRenderService(ContainerBuilder builder);

        protected abstract void RegisterServices(ContainerBuilder builder);

        protected abstract void RegisterPersistenceService(ContainerBuilder builder);
    }
}
