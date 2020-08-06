using System;
using System.Data.SqlClient;
using Autofac;
using ESFA.DC.PeriodEnd.ReportService.Interface.Configuration;
using ESFA.DC.PeriodEnd.ReportService.Reports.Data.DataQuality;
using ESFA.DC.PeriodEnd.ReportService.Reports.DataQuality;
using ESFA.DC.PeriodEnd.ReportService.Reports.Interface.DataQuality;
using ESFA.DC.PeriodEnd.ReportService.Reports.Interface.DataQuality.DataProvider;
using ESFA.DC.PeriodEnd.ReportService.Reports.Interface.ProviderSubmissions;

namespace ESFA.DC.PeriodEnd.ReportService.Modules
{
    public class DataQualityModule : AbstractReportModule<DataQuality>
    {
        private readonly IReportServiceConfiguration _reportServiceConfiguration;

        private readonly string sqlConnectionFuncParameter = "sqlConnectionFunc";

        public DataQualityModule(IReportServiceConfiguration reportServiceConfiguration, IDataPersistConfiguration dataPersistConfiguration) : base(dataPersistConfiguration)
        {
            _reportServiceConfiguration = reportServiceConfiguration;
        }

        protected override void RegisterDataProviders(ContainerBuilder builder)
        {
            var ilrSqlFunc = new Func<SqlConnection>(() =>
                new SqlConnection(_reportServiceConfiguration.ILRDataStoreConnectionString));

            var jobManagementSqlFunc = new Func<SqlConnection>(() =>
                new SqlConnection(_reportServiceConfiguration.JobQueueManagerConnectionString));

            var organisationSqlFunc = new Func<SqlConnection>(() =>
                new SqlConnection(_reportServiceConfiguration.OrgConnectionString));

            var ilrRefSqlFunc = new Func<SqlConnection>(() =>
                new SqlConnection(_reportServiceConfiguration.ILRReferenceDataConnectionString));

            builder.RegisterType<IlrDataProvider>()
                .WithParameter(sqlConnectionFuncParameter, ilrSqlFunc)
                .As<IIlrDataProvider>();

            builder.RegisterType<JobManagementDataProvider>()
                .WithParameter(sqlConnectionFuncParameter, jobManagementSqlFunc)
                .As<IJobManagementDataProvider>();

            builder.RegisterType<OrganisationDataProvider>()
                .WithParameter(sqlConnectionFuncParameter, organisationSqlFunc)
                .As<IOrganisationDataProvider>();

            builder.RegisterType<IlrRefDataProvider>()
                .WithParameter(sqlConnectionFuncParameter, ilrRefSqlFunc)
                .As<IIlrRefDataProvider>();

            builder.RegisterType<DataQualityDataProvider>()
                .As<IDataQualityDataProvider>();
        }

        protected override void RegisterModelBuilder(ContainerBuilder builder)
        {
            builder.RegisterType<DataQualityModelBuilder>().As<IDataQualityModelBuilder>();
        }

        protected override void RegisterRenderService(ContainerBuilder builder)
        {
            builder.RegisterType<DataQualityRenderService>().As<IDataQualityRenderService>();
        }

        protected override void RegisterServices(ContainerBuilder builder)
        {
        }

        protected override void RegisterPersistenceService(ContainerBuilder builder)
        {
        }
    }
}
