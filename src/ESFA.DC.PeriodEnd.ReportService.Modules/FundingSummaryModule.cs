using System.Data.SqlClient;
using Autofac;
using ESFA.DC.PeriodEnd.ReportService.Interface.Configuration;
using ESFA.DC.PeriodEnd.ReportService.Reports.Data.FundingSummary;
using ESFA.DC.PeriodEnd.ReportService.Reports.Data.FundingSummary.Das;
using ESFA.DC.PeriodEnd.ReportService.Reports.Data.FundingSummary.Eas;
using ESFA.DC.PeriodEnd.ReportService.Reports.Data.FundingSummary.Fcs;
using ESFA.DC.PeriodEnd.ReportService.Reports.Data.FundingSummary.Ilr;
using ESFA.DC.PeriodEnd.ReportService.Reports.Data.FundingSummary.Reference;
using ESFA.DC.PeriodEnd.ReportService.Reports.FundingSummary;
using ESFA.DC.PeriodEnd.ReportService.Reports.Interface;
using ESFA.DC.PeriodEnd.ReportService.Reports.Interface.FundingSummary;
using ESFA.DC.PeriodEnd.ReportService.Reports.Interface.FundingSummary.DataProvider;
using ESFA.DC.PeriodEnd.ReportService.Reports.Interface.FundingSummary.Model;
using ESFA.DC.PeriodEnd.ReportService.Reports.Interface.FundingSummary.Persistance;

namespace ESFA.DC.PeriodEnd.ReportService.Modules
{
    public class FundingSummaryModule : Module
    {
        private readonly IReportServiceConfiguration _reportServiceConfiguration;

        public FundingSummaryModule(IReportServiceConfiguration reportServiceConfiguration)
        {
            _reportServiceConfiguration = reportServiceConfiguration;
        }

        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<FundingSummary>().As<IReport>();

            builder.RegisterType<FundingSummaryModelBuilder>().As<IFundingSummaryModelBuilder>();
            builder.RegisterType<FundingSummaryPersistanceMapper>().As<IFundingSummaryPersistanceMapper>();
            builder.RegisterType<FundingSummaryPersistanceService>().As<IFundingSummaryPersistanceService>();
            builder.RegisterType<FundingSummaryRenderService>().As<IRenderService<FundingSummaryReportModel>>();

            builder.RegisterType<PeriodisedValuesLookup>().As<IPeriodisedValuesLookup>();
            builder.RegisterType<FundingSummaryDataProvider>().As<IFundingSummaryDataProvider>();

            RegisterDataProviders(builder);
        }

        private void RegisterDataProviders(ContainerBuilder builder)
        {
            builder.Register(c =>
            {
                SqlConnection dasSqlFunc() => new SqlConnection(_reportServiceConfiguration.DASPaymentsConnectionString);

                return new DasDataProvider(dasSqlFunc);
            }).As<IDasDataProvider>();

            builder.Register(c =>
            {
                SqlConnection dasSqlFunc() => new SqlConnection(_reportServiceConfiguration.DASPaymentsConnectionString);

                SqlConnection easSqlFunc() => new SqlConnection(_reportServiceConfiguration.EasConnectionString);

                return new DasEasDataProvider(dasSqlFunc, easSqlFunc);
            }).As<IDasEasDataProvider>();

            builder.Register(c =>
            {
                SqlConnection easSqlFunc() => new SqlConnection(_reportServiceConfiguration.EasConnectionString);

                return new EasDataProvider(easSqlFunc);
            }).As<IEasDataProvider>();

            builder.Register(c =>
            {
                SqlConnection fcsSqlFunc() => new SqlConnection(_reportServiceConfiguration.FCSConnectionString);

                return new FcsDataProvider(fcsSqlFunc);
            }).As<IFcsDataProvider>();

            builder.Register(c =>
            {
                SqlConnection ilrSqlFunc() =>
                    new SqlConnection(_reportServiceConfiguration.ILRDataStoreConnectionString);

                return new Fm25DataProvider(ilrSqlFunc);
            }).As<IFm25DataProvider>();

            builder.Register(c =>
            {
                SqlConnection ilrSqlFunc() =>
                    new SqlConnection(_reportServiceConfiguration.ILRDataStoreConnectionString);

                return new Fm35DataProvider(ilrSqlFunc);
            }).As<IFm35DataProvider>();

            builder.Register(c =>
            {
                SqlConnection ilrSqlFunc() =>
                    new SqlConnection(_reportServiceConfiguration.ILRDataStoreConnectionString);

                return new Fm81DataProvider(ilrSqlFunc);
            }).As<IFm81DataProvider>();

            builder.Register(c =>
            {
                SqlConnection ilrSqlFunc() =>
                    new SqlConnection(_reportServiceConfiguration.ILRDataStoreConnectionString);

                return new Fm99DataProvider(ilrSqlFunc);
            }).As<IFm99DataProvider>();

            builder.Register(c =>
            {
                SqlConnection ilrSqlFunc() =>
                    new SqlConnection(_reportServiceConfiguration.ILRDataStoreConnectionString);

                SqlConnection orgSqlFunc() =>
                    new SqlConnection(_reportServiceConfiguration.OrgConnectionString);

                SqlConnection easSqlFunc() =>
                    new SqlConnection(_reportServiceConfiguration.EasConnectionString);

                return new ReferenceDataProvider(orgSqlFunc, easSqlFunc, ilrSqlFunc);
            }).As<IReferenceDataProvider>();
        }
    }
}
