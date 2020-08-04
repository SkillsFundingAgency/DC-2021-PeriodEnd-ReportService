using System;
using System.Data.SqlClient;
using Autofac;
using ESFA.DC.PeriodEnd.ReportService.Interface.Configuration;
using ESFA.DC.PeriodEnd.ReportService.Reports.Data.ProviderSubmissions;
using ESFA.DC.PeriodEnd.ReportService.Reports.Interface.ProviderSubmissions;
using ESFA.DC.PeriodEnd.ReportService.Reports.Interface.ProviderSubmissions.DataProvider;
using ESFA.DC.PeriodEnd.ReportService.Reports.ProviderSubmissions;

namespace ESFA.DC.PeriodEnd.ReportService.Modules
{
    public class ProviderSubmissionModule : AbstractReportModule<ProviderSubmission>
    {
        private readonly IReportServiceConfiguration _reportServiceConfiguration;

        private readonly string sqlConnectionFuncParameter = "sqlConnectionFunc";

        public ProviderSubmissionModule(IReportServiceConfiguration reportServiceConfiguration, IDataPersistConfiguration dataPersistConfiguration) : base(dataPersistConfiguration)
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

            builder.RegisterType<IlrDataProvider>()
                .WithParameter(sqlConnectionFuncParameter, ilrSqlFunc)
                .As<IIlrDataProvider>();

            builder.RegisterType<JobManagementDataProvider>()
                .WithParameter(sqlConnectionFuncParameter, jobManagementSqlFunc)
                .As<IJobManagementDataProvider>();

            builder.RegisterType<OrganisationDataProvider>()
                .WithParameter(sqlConnectionFuncParameter, organisationSqlFunc)
                .As<IOrganisationDataProvider>();

            builder.RegisterType<ProviderSubmissionsDataProvider>()
                .As<IProviderSubmissionsDataProvider>();
        }

        protected override void RegisterModelBuilder(ContainerBuilder builder)
        {
            builder.RegisterType<ProviderSubmissionsModelBuilder>().As<IProviderSubmissionsModelBuilder>();
        }

        protected override void RegisterRenderService(ContainerBuilder builder)
        {
            builder.RegisterType<ProviderSubmissionsRenderService>().As<IProviderSubmissionsRenderService>();
        }

        protected override void RegisterServices(ContainerBuilder builder)
        {
        }

        protected override void RegisterPersistenceService(ContainerBuilder builder)
        {
        }
    }
}
