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
using ESFA.DC.PeriodEnd.ReportService.Reports.Interface.AppsCoInvestment;
using ESFA.DC.PeriodEnd.ReportService.Reports.Interface.AppsCoInvestment.Builders;
using ESFA.DC.PeriodEnd.ReportService.Reports.Interface.AppsCoInvestment.DataProvider;
using ESFA.DC.PeriodEnd.ReportService.Reports.Interface.AppsCoInvestment.Model;
using ESFA.DC.PeriodEnd.ReportService.Reports.Interface.AppsCoInvestment.Persistence;
using ESFA.DC.ReportData.Model;
using Microsoft.EntityFrameworkCore;

namespace ESFA.DC.PeriodEnd.ReportService.Modules
{
    public class AppsCoInvestmentModule : AbstractReportModule<AppsCoInvestment>
    {
        private readonly IReportServiceConfiguration _reportServiceConfiguration;

        public AppsCoInvestmentModule(IReportServiceConfiguration reportServiceConfiguration, IDataPersistConfiguration dataPersistConfiguration) 
            : base(dataPersistConfiguration)
        {
            _reportServiceConfiguration = reportServiceConfiguration;
        }

        protected override void RegisterDataProviders(ContainerBuilder builder)
        {
            builder.RegisterType<AppsCoInvestmentDataProvider>().As<IAppsCoInvestmentDataProvider>();
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

        protected override void RegisterModelBuilder(ContainerBuilder builder)
        {
            builder.RegisterType<AppsCoInvestmentModelBuilder>().As<IAppsCoInvestmentModelBuilder>();
        }

        protected override void RegisterRenderService(ContainerBuilder builder)
        {
        }

        protected override void RegisterServices(ContainerBuilder builder)
        {
            builder.RegisterType<AppsCoInvestmentRecordKeyEqualityComparer>().As<IEqualityComparer<AppsCoInvestmentRecordKey>>().InstancePerLifetimeScope();

            builder.RegisterType<PaymentsBuilder>().As<IPaymentsBuilder>();
            builder.RegisterType<LearnersBuilder>().As<ILearnersBuilder>();
            builder.RegisterType<LearningDeliveriesBuilder>().As<ILearningDeliveriesBuilder>();
        }

        protected override void RegisterPersistenceService(ContainerBuilder builder)
            => RegisterPersistenceService<AppsCoInvestmentPersistenceMapper, IAppsCoInvestmentPersistenceMapper, AppsCoInvestmentContribution>(builder, TableNameConstants.AppsCoInvestmentContributions);
    }
}
