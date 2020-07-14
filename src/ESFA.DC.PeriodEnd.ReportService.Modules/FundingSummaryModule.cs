using System;
using System.Data.SqlClient;
using Autofac;
using ESFA.DC.PeriodEnd.ReportService.Interface.Configuration;
using ESFA.DC.PeriodEnd.ReportService.Reports.Constants;
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
using ESFA.DC.ReportData.Model;

namespace ESFA.DC.PeriodEnd.ReportService.Modules
{
    public class FundingSummaryModule : AbstractReportModule<FundingSummary>
    {
        private readonly IReportServiceConfiguration _reportServiceConfiguration;

        private const string sqlConnectionFuncParameter = "sqlConnectionFunc";
        private const string dasSqlFuncParameter = "dasSqlFunc";
        private const string easSqlFuncParameter = "easSqlFunc";
        private const string orgSqlFuncParameter = "orgSqlFunc";
        private const string ilrSqlFuncParameter = "ilrSqlFunc";

        public FundingSummaryModule(IReportServiceConfiguration reportServiceConfiguration, IDataPersistConfiguration dataPersistConfiguration) 
            : base(dataPersistConfiguration)
        {
            _reportServiceConfiguration = reportServiceConfiguration;
        }

        protected override void RegisterModelBuilder(ContainerBuilder builder)
        {
            builder.RegisterType<FundingSummaryModelBuilder>().As<IFundingSummaryModelBuilder>();
        }

        protected override void RegisterRenderService(ContainerBuilder builder)
        {
            builder.RegisterType<FundingSummaryRenderService>().As<IRenderService<FundingSummaryReportModel>>();
        }

        protected override void RegisterServices(ContainerBuilder builder)
        {
            builder.RegisterType<PeriodisedValuesLookup>().As<IPeriodisedValuesLookup>();
        }

        protected override void RegisterDataProviders(ContainerBuilder builder)
        {
            var dasSqlFunc = new Func<SqlConnection>(() =>
                new SqlConnection(_reportServiceConfiguration.DASPaymentsConnectionString));

            var easSqlFunc = new Func<SqlConnection>(() => new SqlConnection(_reportServiceConfiguration.EasConnectionString));

            var fcsSqlFunc = new Func<SqlConnection>(() => new SqlConnection(_reportServiceConfiguration.FCSConnectionString));

            var ilrSqlFunc = new Func<SqlConnection>(() =>
                new SqlConnection(_reportServiceConfiguration.ILRDataStoreConnectionString));

            var orgSqlFunc = new Func<SqlConnection>(() => new SqlConnection(_reportServiceConfiguration.OrgConnectionString));

            builder.RegisterType<DasDataProvider>()
                .WithParameter(sqlConnectionFuncParameter, dasSqlFunc)
                .As<IDasDataProvider>();

            builder.RegisterType<EasDataProvider>()
                .WithParameter(sqlConnectionFuncParameter, easSqlFunc)
                .As<IEasDataProvider>();

            builder.RegisterType<FcsDataProvider>()
                .WithParameter(sqlConnectionFuncParameter, fcsSqlFunc)
                .As<IFcsDataProvider>();

            builder.RegisterType<Fm25DataProvider>()
                .WithParameter(sqlConnectionFuncParameter, ilrSqlFunc)
                .As<IFm25DataProvider>();

            builder.RegisterType<Fm35DataProvider>()
                .WithParameter(sqlConnectionFuncParameter, ilrSqlFunc)
                .As<IFm35DataProvider>();

            builder.RegisterType<Fm81DataProvider>()
                .WithParameter(sqlConnectionFuncParameter, ilrSqlFunc)
                .As<IFm81DataProvider>();

            builder.RegisterType<Fm99DataProvider>()
                .WithParameter(sqlConnectionFuncParameter, ilrSqlFunc)
                .As<IFm99DataProvider>();

            builder.RegisterType<DasEasDataProvider>()
                .WithParameter(dasSqlFuncParameter, dasSqlFunc)
                .WithParameter(easSqlFuncParameter, easSqlFunc)
                .As<IDasEasDataProvider>();

            builder.RegisterType<ReferenceDataProvider>()
                .WithParameter(orgSqlFuncParameter, orgSqlFunc)
                .WithParameter(easSqlFuncParameter, easSqlFunc)
                .WithParameter(ilrSqlFuncParameter, ilrSqlFunc)
                .As<IReferenceDataProvider>();

            builder.RegisterType<FundingSummaryDataProvider>().As<IFundingSummaryDataProvider>();
        }

        protected override void RegisterPersistenceService(ContainerBuilder builder)
            => RegisterPersistenceService<FundingSummaryPersistanceMapper, IFundingSummaryPersistanceMapper, FundingSummaryReport>(builder, TableNameConstants.FundingSummaryReport);
    }
}
