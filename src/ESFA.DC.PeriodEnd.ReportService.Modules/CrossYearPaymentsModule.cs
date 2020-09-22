using System;
using System.Data.SqlClient;
using Autofac;
using ESFA.DC.PeriodEnd.ReportService.Interface.Configuration;
using ESFA.DC.PeriodEnd.ReportService.Reports.CrossYearPayments;
using ESFA.DC.PeriodEnd.ReportService.Reports.Data.CrossYearPayments;
using ESFA.DC.PeriodEnd.ReportService.Reports.Data.CrossYearPayments.Apps;
using ESFA.DC.PeriodEnd.ReportService.Reports.Data.CrossYearPayments.Fcs;
using ESFA.DC.PeriodEnd.ReportService.Reports.Data.CrossYearPayments.Org;
using ESFA.DC.PeriodEnd.ReportService.Reports.Interface.CrossYearPayments;
using ESFA.DC.PeriodEnd.ReportService.Reports.Interface.CrossYearPayments.DataProvider;

namespace ESFA.DC.PeriodEnd.ReportService.Modules
{
    public class CrossYearPaymentsModule : AbstractReportModule<CrossYearPayments>
    {
        private readonly IReportServiceConfiguration _reportServiceConfiguration;

        private const string SqlConnectionFuncParameter = "sqlConnectionFunc";

        public CrossYearPaymentsModule(IReportServiceConfiguration reportServiceConfiguration, IDataPersistConfiguration dataPersistConfiguration) : base(dataPersistConfiguration)
        {
            _reportServiceConfiguration = reportServiceConfiguration;
        }

        protected override void RegisterDataProviders(ContainerBuilder builder)
        {
            var orgSqlFunc = new Func<SqlConnection>(() => new SqlConnection(_reportServiceConfiguration.OrgConnectionString));
            var fcsSqlFunc = new Func<SqlConnection>(() => new SqlConnection(_reportServiceConfiguration.FCSConnectionString));
            var appsSqlFunc = new Func<SqlConnection>(() => new SqlConnection(_reportServiceConfiguration.DASPaymentsConnectionString));

            builder.RegisterType<CrossYearPaymentsDataProvider>().As<ICrossYearDataProvider>();

            builder.RegisterType<OrgDataProvider>()
                .WithParameter(SqlConnectionFuncParameter, orgSqlFunc)
                .As<IOrgDataProvider>();

            builder.RegisterType<FcsDataProvider>()
                .WithParameter(SqlConnectionFuncParameter, fcsSqlFunc)
                .As<IFcsDataProvider>();

            builder.RegisterType<AppsDataProvider>()
                .WithParameter(SqlConnectionFuncParameter, appsSqlFunc)
                .As<IAppsDataProvider>();
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
