using System;
using System.Data.SqlClient;
using Autofac;
using ESFA.DC.PeriodEnd.ReportService.Interface.Configuration;
using ESFA.DC.PeriodEnd.ReportService.Reports.ActCount;
using ESFA.DC.PeriodEnd.ReportService.Reports.Data.ActCount;
using ESFA.DC.PeriodEnd.ReportService.Reports.Interface.ActCount;
using ESFA.DC.PeriodEnd.ReportService.Reports.Interface.ActCount.DataProvider;

namespace ESFA.DC.PeriodEnd.ReportService.Modules
{
    public class ActCountReportModule : AbstractReportModule<ActCount>
    {
        private readonly IReportServiceConfiguration _reportServiceConfiguration;

        private readonly string sqlConnectionFuncParameter = "sqlConnectionFunc";

        public ActCountReportModule(IReportServiceConfiguration reportServiceConfiguration, IDataPersistConfiguration dataPersistConfiguration) 
            : base(dataPersistConfiguration)
        {
            _reportServiceConfiguration = reportServiceConfiguration;
        }

        protected override void RegisterDataProviders(ContainerBuilder builder)
        {
            var ilrSqlFunc = new Func<SqlConnection>(() =>
                new SqlConnection(_reportServiceConfiguration.ILRDataStoreConnectionString));

            builder.RegisterType<ActCountDataProvider>()
                .WithParameter(sqlConnectionFuncParameter, ilrSqlFunc)
                .As<IActCountDataProvider>();
        }

        protected override void RegisterModelBuilder(ContainerBuilder builder)
        {
            builder.RegisterType<ActCountModelBuilder>().As<IActCountModelBuilder>();
        }

        protected override void RegisterRenderService(ContainerBuilder builder)
        {
        }

        protected override void RegisterServices(ContainerBuilder builder)
        {
        }

        protected override void RegisterPersistenceService(ContainerBuilder builder)
        {
        }
    }
}
