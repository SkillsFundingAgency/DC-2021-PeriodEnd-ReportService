using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using Autofac;
using ESFA.DC.ILR2021.DataStore.EF;
using ESFA.DC.ILR2021.DataStore.EF.Interface;
using ESFA.DC.PeriodEnd.DataPersist;
using ESFA.DC.PeriodEnd.ReportService.Interface.Configuration;
using ESFA.DC.PeriodEnd.ReportService.Reports.AppsCoInvestment;
using ESFA.DC.PeriodEnd.ReportService.Reports.AppsCoInvestment.Builders;
using ESFA.DC.PeriodEnd.ReportService.Reports.AppsCoInvestment.Comparer;
using ESFA.DC.PeriodEnd.ReportService.Reports.Data.AppsCoInvestment;
using ESFA.DC.PeriodEnd.ReportService.Reports.Data.AppsCoInvestment.Das;
using ESFA.DC.PeriodEnd.ReportService.Reports.Data.AppsCoInvestment.Ilr;
using ESFA.DC.PeriodEnd.ReportService.Reports.Interface;
using ESFA.DC.PeriodEnd.ReportService.Reports.Interface.AppsCoInvestment;
using ESFA.DC.PeriodEnd.ReportService.Reports.Interface.AppsCoInvestment.Builders;
using ESFA.DC.PeriodEnd.ReportService.Reports.Interface.AppsCoInvestment.DataProvider;
using ESFA.DC.PeriodEnd.ReportService.Reports.Interface.AppsCoInvestment.Model;
using ESFA.DC.PeriodEnd.ReportService.Reports.Interface.AppsCoInvestment.Persistence;
using ESFA.DC.PeriodEnd.ReportService.Reports.Persist;
using ESFA.DC.ReportData.Model;
using Microsoft.EntityFrameworkCore;

namespace ESFA.DC.PeriodEnd.ReportService.Modules
{
    public class AppsCoInvestmentModule : Module
    {
        private readonly IReportServiceConfiguration _reportServiceConfiguration;
        private readonly IDataPersistConfiguration _dataPersistConfiguration;
        private const string tableNameParameter = "tableName";

        public AppsCoInvestmentModule(IReportServiceConfiguration reportServiceConfiguration, IDataPersistConfiguration dataPersistConfiguration)
        {
            _reportServiceConfiguration = reportServiceConfiguration;
            _dataPersistConfiguration = dataPersistConfiguration;
        }

        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<AppsCoInvestment>().As<IReport>();

            builder.RegisterType<AppsCoInvestmentDataProvider>().As<IAppsCoInvestmentDataProvider>();
            builder.RegisterType<AppsCoInvestmentModelBuilder>().As<IAppsCoInvestmentModelBuilder>();
            builder.RegisterType<AppsCoInvestmentRecordKeyEqualityComparer>().As<IEqualityComparer<AppsCoInvestmentRecordKey>>().InstancePerLifetimeScope();
            builder.RegisterType<AppsCoInvestmentPersistenceMapper>().As<IAppsCoInvestmentPersistenceMapper>();
            //builders
            builder.RegisterType<PaymentsBuilder>().As<IPaymentsBuilder>();
            builder.RegisterType<LearnersBuilder>().As<ILearnersBuilder>();
            builder.RegisterType<LearningDeliveriesBuilder>().As<ILearningDeliveriesBuilder>();

            var sqlFunc = new Func<SqlConnection>(() =>
                new SqlConnection(_dataPersistConfiguration.ReportDataConnectionString));

            builder.RegisterType<ReportDataPersistanceService<AppsCoInvestmentContribution>>()
                .WithParameter("sqlConnectionFunc", sqlFunc)
                .WithParameter(tableNameParameter, TableNameConstants.AppsCoInvestmentContributions)
                .As<IReportDataPersistanceService<AppsCoInvestmentContribution>>();

            RegisterDataProviders(builder);
        }

        private void RegisterDataProviders(ContainerBuilder builder)
        {
            builder.RegisterType<LearnerDataProvider>().As<ILearnerDataProvider>();

            builder.Register(c =>
            {
                SqlConnection PaymentsSqlFunc() => new SqlConnection(_reportServiceConfiguration.DASPaymentsConnectionString);

                return new PaymentsDataProvider(PaymentsSqlFunc);
            }).As<IPaymentsDataProvider>();

            builder.Register(c =>
            {
                SqlConnection IlrSqlFunc() => new SqlConnection(_reportServiceConfiguration.ILRDataStoreConnectionString);

                return new IlrDataProvider(IlrSqlFunc);
            }).As<IIlrDataProvider>();

            builder.RegisterType<ILR2021_DataStoreEntities>().As<IIlr2021Context>();
            builder.Register(context =>
            {
                var optionsBuilder = new DbContextOptionsBuilder<ILR2021_DataStoreEntities>();
                optionsBuilder.UseSqlServer(_reportServiceConfiguration.ILRDataStoreConnectionString, options => options.EnableRetryOnFailure(3, TimeSpan.FromSeconds(3), new List<int>()));

                return optionsBuilder.Options;
            }).As<DbContextOptions<ILR2021_DataStoreEntities>>()
                .SingleInstance();
        }
    }
}
