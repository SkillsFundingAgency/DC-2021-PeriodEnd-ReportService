using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Text;
using Autofac;
using ESFA.DC.PeriodEnd.ReportService.Interface.Configuration;
using ESFA.DC.PeriodEnd.ReportService.Reports.CollectionStats;
using ESFA.DC.PeriodEnd.ReportService.Reports.Data.CollectionStats;
using ESFA.DC.PeriodEnd.ReportService.Reports.Interface.CollectionStats;
using ESFA.DC.PeriodEnd.ReportService.Reports.Interface.CollectionStats.DataProvider;

namespace ESFA.DC.PeriodEnd.ReportService.Modules
{
    public class CollectionStatsModule : AbstractReportModule<CollectionStats>
    {
        private readonly IReportServiceConfiguration _reportServiceConfiguration;

        private readonly string sqlConnectionFuncParameter = "sqlConnectionFunc";

        public CollectionStatsModule(IReportServiceConfiguration reportServiceConfiguration, IDataPersistConfiguration dataPersistConfiguration) : base(dataPersistConfiguration)
        {
            _reportServiceConfiguration = reportServiceConfiguration;
        }

        protected override void RegisterDataProviders(ContainerBuilder builder)
        {
            var jmSqlFunc = new Func<SqlConnection>(() =>
                new SqlConnection(_reportServiceConfiguration.JobQueueManagerConnectionString));

            builder.RegisterType<CollectionStatsDataProvider>()
                .WithParameter(sqlConnectionFuncParameter, jmSqlFunc)
                .As<ICollectionStatsDataProvider>();
        }

        protected override void RegisterModelBuilder(ContainerBuilder builder)
        {
            builder.RegisterType<CollectionStatsModelBuilder>().As<ICollectionStatsModelBuilder>();
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
