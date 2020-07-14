using System;
using System.Data.SqlClient;
using Autofac;
using ESFA.DC.PeriodEnd.ReportService.Interface.Configuration;
using ESFA.DC.PeriodEnd.ReportService.Reports.Interface;
using ESFA.DC.PeriodEnd.ReportService.Reports.Persist;

namespace ESFA.DC.PeriodEnd.ReportService.Modules
{
    public abstract class AbstractReportModule<T> : Module where T : IReport
    {
        protected readonly IDataPersistConfiguration DataPersistConfiguration;

        private const string tableNameParameter = "tableName";
        private const string sqlConnectionFuncParameter = "sqlConnectionFunc";

        protected AbstractReportModule(IDataPersistConfiguration dataPersistConfiguration)
        {
            DataPersistConfiguration = dataPersistConfiguration;
        }

        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<T>().As<IReport>();

            RegisterDataProviders(builder);

            RegisterModelBuilder(builder);

            RegisterRenderService(builder);

            RegisterServices(builder);

            RegisterPersistenceService(builder);
        }

        protected abstract void RegisterDataProviders(ContainerBuilder builder);

        protected abstract void RegisterModelBuilder(ContainerBuilder builder);

        protected abstract void RegisterRenderService(ContainerBuilder builder);

        protected abstract void RegisterServices(ContainerBuilder builder);

        protected abstract void RegisterPersistenceService(ContainerBuilder builder);

        protected void RegisterPersistenceService<TMapper, TMapperInterface, TDataClass>(ContainerBuilder builder, string tableName)
        {
            builder.RegisterType<TMapper>().As<TMapperInterface>();

            var sqlFunc = new Func<SqlConnection>(() =>
                new SqlConnection(DataPersistConfiguration.ReportDataConnectionString));

            builder.RegisterType<ReportDataPersistanceService<TDataClass>>()
                .WithParameter(sqlConnectionFuncParameter, sqlFunc)
                .WithParameter(tableNameParameter, tableName)
                .As<IReportDataPersistanceService<TDataClass>>();
        }
    }
}
