using Autofac;
using ESFA.DC.PeriodEnd.ReportService.Interface.Configuration;
using ESFA.DC.PeriodEnd.ReportService.Reports.CrossYearPayments;
using ESFA.DC.PeriodEnd.ReportService.Reports.Data.CrossYearPayments;
using ESFA.DC.PeriodEnd.ReportService.Reports.Interface.CrossYearPayments;

namespace ESFA.DC.PeriodEnd.ReportService.Modules
{
    public class CrossYearPaymentsModule : AbstractReportModule<CrossYearPayments>
    {
        public CrossYearPaymentsModule(IDataPersistConfiguration dataPersistConfiguration) : base(dataPersistConfiguration)
        {
        }

        protected override void RegisterDataProviders(ContainerBuilder builder)
        {
            builder.RegisterType<CrossYearPaymentsDataProvider>().As<ICrossYearDataProvider>();
        }

        protected override void RegisterModelBuilder(ContainerBuilder builder)
        {
            builder.RegisterType<CrossYearPaymentsModelBuilder>().As<ICrossYearModelBuilder>();
        }

        protected override void RegisterRenderService(ContainerBuilder builder)
        {
            builder.RegisterType<CrossYearPaymentsRenderService>().As<ICrossYearRenderService>();
        }

        protected override void RegisterServices(ContainerBuilder builder)
        {
        }

        protected override void RegisterPersistenceService(ContainerBuilder builder)
        {
        }
    }
}
