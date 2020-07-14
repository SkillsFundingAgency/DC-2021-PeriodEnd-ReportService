using System.Data.SqlClient;
using Autofac;
using ESFA.DC.PeriodEnd.DataPersist;
using ESFA.DC.PeriodEnd.ReportService.Interface.Configuration;
using ESFA.DC.PeriodEnd.ReportService.Reports.AppsAdditionalPayments;
using ESFA.DC.PeriodEnd.ReportService.Reports.AppsAdditionalPayments.Interface;
using ESFA.DC.PeriodEnd.ReportService.Reports.Data.AppsAdditionalPayments;
using ESFA.DC.PeriodEnd.ReportService.Reports.Data.AppsAdditionalPayments.Das;
using ESFA.DC.PeriodEnd.ReportService.Reports.Data.AppsAdditionalPayments.Ilr;
using ESFA.DC.PeriodEnd.ReportService.Reports.Interface.AppsAdditionalPayments;
using ESFA.DC.PeriodEnd.ReportService.Reports.Interface.AppsAdditionalPayments.DataProvider;
using ESFA.DC.PeriodEnd.ReportService.Reports.Interface.AppsAdditionalPayments.Persistance;

namespace ESFA.DC.PeriodEnd.ReportService.Modules
{
    public class AppsAdditionalPaymentsModule : AbstractReportModule<AppsAdditionalPayment>
    {
        private readonly IReportServiceConfiguration _reportServiceConfiguration;

        public AppsAdditionalPaymentsModule(IReportServiceConfiguration reportServiceConfiguration, IDataPersistConfiguration dataPersistConfiguration) 
            : base(dataPersistConfiguration)
        {
            _reportServiceConfiguration = reportServiceConfiguration;
        }

        protected override void RegisterDataProviders(ContainerBuilder builder)
        {
            builder.RegisterType<AppsAdditionalPaymentsDataProvider>().As<IAppsAdditionalPaymentsDataProvider>();

            builder.Register(c =>
            {
                SqlConnection dasSqlFunc() => new SqlConnection(_reportServiceConfiguration.DASPaymentsConnectionString);

                return new PaymentsDataProvider(dasSqlFunc);
            }).As<IPaymentsDataProvider>();

            builder.Register(c =>
            {
                SqlConnection ilrSqlFunc() =>
                    new SqlConnection(_reportServiceConfiguration.ILRDataStoreConnectionString);

                return new LearnerDataProvider(ilrSqlFunc);
            }).As<ILearnerDataProvider>();

            builder.Register(c =>
            {
                SqlConnection ilrSqlFunc() =>
                    new SqlConnection(_reportServiceConfiguration.ILRDataStoreConnectionString);

                return new AecLearningDeliveryDataProvider(ilrSqlFunc);
            }).As<IAecLearningDeliveryDataProvider>();

            builder.Register(c =>
            {
                SqlConnection ilrSqlFunc() =>
                    new SqlConnection(_reportServiceConfiguration.ILRDataStoreConnectionString);

                return new AppsPriceEpisodePeriodisedValuesDataProvider(ilrSqlFunc);
            }).As<IAppsPriceEpisodePeriodisedValuesDataProvider>();
        }

        protected override void RegisterModelBuilder(ContainerBuilder builder)
        {
            builder.RegisterType<AppsAdditionalPaymentsModelBuilder>().As<IAppsAdditionalPaymentsModelBuilder>();
        }

        protected override void RegisterRenderService(ContainerBuilder builder)
        {
        }

        protected override void RegisterServices(ContainerBuilder builder)
        {
            builder.RegisterType<EarningsAndPaymentsBuilder>().As<IEarningsAndPaymentsBuilder>();
            builder.RegisterType<PaymentLineFormatter>().As<IPaymentLineFormatter>();
        }

        protected override void RegisterPersistenceService(ContainerBuilder builder)
            => RegisterPersistenceService<AppsAdditionalPaymentPersistanceMapper, IAppsAdditionalPaymentPersistanceMapper, ReportData.Model.AppsAdditionalPayment>(builder, TableNameConstants.AppsAdditionalPayments);
    }
}