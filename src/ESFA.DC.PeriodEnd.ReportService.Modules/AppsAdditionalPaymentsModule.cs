using System.Data.SqlClient;
using Autofac;
using ESFA.DC.PeriodEnd.ReportService.Interface.Configuration;
using ESFA.DC.PeriodEnd.ReportService.Reports.AppsAdditionalPayments;
using ESFA.DC.PeriodEnd.ReportService.Reports.AppsAdditionalPayments.Interface;
using ESFA.DC.PeriodEnd.ReportService.Reports.Data.AppsAdditionalPayments;
using ESFA.DC.PeriodEnd.ReportService.Reports.Data.AppsAdditionalPayments.Das;
using ESFA.DC.PeriodEnd.ReportService.Reports.Data.AppsAdditionalPayments.Ilr;
using ESFA.DC.PeriodEnd.ReportService.Reports.Interface;
using ESFA.DC.PeriodEnd.ReportService.Reports.Interface.AppsAdditionalPayments;
using ESFA.DC.PeriodEnd.ReportService.Reports.Interface.AppsAdditionalPayments.DataProvider;

namespace ESFA.DC.PeriodEnd.ReportService.Modules
{
    public class AppsAdditionalPaymentsModule : Module
    {
        private readonly IReportServiceConfiguration _reportServiceConfiguration;

        public AppsAdditionalPaymentsModule(IReportServiceConfiguration reportServiceConfiguration)
        {
            _reportServiceConfiguration = reportServiceConfiguration;
        }

        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<AppsAdditionalPayment>().As<IReport>();

            builder.RegisterType<PaymentLineFormatter>().As<IPaymentLineFormatter>();
            builder.RegisterType<EarningsAndPaymentsBuilder>().As<IEarningsAndPaymentsBuilder>();
            builder.RegisterType<AppsAdditionalPaymentsModelBuilder>().As<IAppsAdditionalPaymentsModelBuilder>();

            builder.RegisterType<AppsAdditionalPaymentsDataProvider>().As<IAppsAdditionalPaymentsDataProvider>();

            RegisterDataProviders(builder);
        }

        private void RegisterDataProviders(ContainerBuilder builder)
        {
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
    }
}