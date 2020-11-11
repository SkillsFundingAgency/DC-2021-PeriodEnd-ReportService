using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using Autofac;
using ESFA.DC.ILR2021.DataStore.EF;
using ESFA.DC.ILR2021.DataStore.EF.Interface;
using ESFA.DC.PeriodEnd.DataPersist;
using ESFA.DC.PeriodEnd.ReportService.Interface.Configuration;
using ESFA.DC.PeriodEnd.ReportService.Reports.AppsMonthly;
using ESFA.DC.PeriodEnd.ReportService.Reports.Data.UYPSummaryView;
using ESFA.DC.PeriodEnd.ReportService.Reports.Data.UYPSummaryView.Das;
using ESFA.DC.PeriodEnd.ReportService.Reports.Data.UYPSummaryView.ILR;
using ESFA.DC.PeriodEnd.ReportService.Reports.Interface.UYPSummaryView;
using ESFA.DC.PeriodEnd.ReportService.Reports.Interface.UYPSummaryView.DataProvider;
using ESFA.DC.PeriodEnd.ReportService.Reports.Interface.UYPSummaryView.Model.Comparer;
using ESFA.DC.PeriodEnd.ReportService.Reports.Interface.UYPSummaryView.Persistence;
using ESFA.DC.PeriodEnd.ReportService.Reports.UYPSummaryView;
using ESFA.DC.PeriodEnd.ReportService.Reports.UYPSummaryView.Interface;
using ESFA.DC.ReportData.Model;
using ESFA.DC.Serialization.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ESFA.DC.PeriodEnd.ReportService.Modules
{
    public class UYPSummaryViewModule : AbstractReportModule<UYPSummaryView>
    {
        private readonly IReportServiceConfiguration _reportServiceConfiguration;

        public UYPSummaryViewModule(IReportServiceConfiguration reportServiceConfiguration, IDataPersistConfiguration dataPersistConfiguration) 
            : base(dataPersistConfiguration)
        {
            _reportServiceConfiguration = reportServiceConfiguration;
        }

        protected override void RegisterDataProviders(ContainerBuilder builder)
        {
            builder.RegisterType<UYPSummaryViewDataProvider>().As<IUYPSummaryViewDataProvider>();
            builder.Register(c =>
            {
                SqlConnection PaymentsSqlFunc() => new SqlConnection(_reportServiceConfiguration.DASPaymentsConnectionString);

                return new PaymentsDataProvider(PaymentsSqlFunc);
            }).As<IPaymentsDataProvider>();

            builder.Register(c =>
            {
                SqlConnection IlrSqlFunc() => new SqlConnection(_reportServiceConfiguration.ILRDataStoreConnectionString);

                return new ILRPaymentsDataProvider(IlrSqlFunc);
            }).As<IILRPaymentsDataProvider>();
        }

        protected override void RegisterModelBuilder(ContainerBuilder builder)
        {
            builder.RegisterType<UYPSummaryViewModelBuilder>().As<IUYPSummaryViewModelBuilder>();
        }

        protected override void RegisterRenderService(ContainerBuilder builder)
        {
        }

        protected override void RegisterServices(ContainerBuilder builder)
        {
            builder.RegisterType<LLVPaymentRecordKeyEqualityComparer>().As<ILLVPaymentRecordKeyEqualityComparer>().InstancePerLifetimeScope();
            builder.RegisterType<LLVPaymentRecordLRefOnlyKeyEqualityComparer>().As<ILLVPaymentRecordLRefOnlyKeyEqualityComparer>().InstancePerLifetimeScope();
        }

        protected override void RegisterPersistenceService(ContainerBuilder builder)
        {
            RegisterPersistenceService<UYPSummaryViewPersistenceMapper, IUYPSummaryViewPersistenceMapper,
                LearnerLevelViewReport>(builder, TableNameConstants.LearnerLevelViewReport);

            RegisterPersistenceService<UYPSummaryViewPersistenceMapper, IUYPSummaryViewPersistenceMapper,
                UYPSummaryViewReport>(builder, TableNameConstants.UYPSummaryViewReport);
        }
    }
}   