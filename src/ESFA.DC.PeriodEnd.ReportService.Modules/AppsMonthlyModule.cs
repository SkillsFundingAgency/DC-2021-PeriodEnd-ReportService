﻿using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using Autofac;
using ESFA.DC.ILR2021.DataStore.EF;
using ESFA.DC.ILR2021.DataStore.EF.Interface;
using ESFA.DC.PeriodEnd.DataPersist;
using ESFA.DC.PeriodEnd.ReportService.Interface.Configuration;
using ESFA.DC.PeriodEnd.ReportService.Reports.AppsMonthly;
using ESFA.DC.PeriodEnd.ReportService.Reports.AppsMonthly.Interface;
using ESFA.DC.PeriodEnd.ReportService.Reports.Data.AppsMonthly;
using ESFA.DC.PeriodEnd.ReportService.Reports.Data.AppsMonthly.Das;
using ESFA.DC.PeriodEnd.ReportService.Reports.Data.AppsMonthly.Fcs;
using ESFA.DC.PeriodEnd.ReportService.Reports.Data.AppsMonthly.Ilr;
using ESFA.DC.PeriodEnd.ReportService.Reports.Data.AppsMonthly.Lars;
using ESFA.DC.PeriodEnd.ReportService.Reports.Interface;
using ESFA.DC.PeriodEnd.ReportService.Reports.Interface.AppsMonthly;
using ESFA.DC.PeriodEnd.ReportService.Reports.Interface.AppsMonthly.DataProvider;
using ESFA.DC.PeriodEnd.ReportService.Reports.Interface.AppsMonthly.Persistence;
using ESFA.DC.PeriodEnd.ReportService.Reports.Persist;
using ESFA.DC.ReportData.Model;
using ESFA.DC.Serialization.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ESFA.DC.PeriodEnd.ReportService.Modules
{
    public class AppsMonthlyModule : Module
    {
        private readonly IReportServiceConfiguration _reportServiceConfiguration;
        private readonly IDataPersistConfiguration _dataPersistConfiguration;
        private const string tableNameParameter = "tableName";

        public AppsMonthlyModule(IReportServiceConfiguration reportServiceConfiguration, IDataPersistConfiguration dataPersistConfiguration)
        {
            _reportServiceConfiguration = reportServiceConfiguration;
            _dataPersistConfiguration = dataPersistConfiguration;
        }

        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<AppsMonthly>().As<IReport>();

            builder.RegisterType<AppsMonthlyPaymentsDataProvider>().As<IAppsMonthlyPaymentsDataProvider>();
            builder.RegisterType<AppsMonthlyModelBuilder>().As<IAppsMonthlyModelBuilder>();
            builder.RegisterType<LearningDeliveryFamsBuilder>().As<ILearningDeliveryFamsBuilder>();
            builder.RegisterType<PaymentPeriodsBuilder>().As<IPaymentPeriodsBuilder>();
            builder.RegisterType<ProviderMonitoringsBuilder>().As<IProviderMonitoringsBuilder>();
            builder.RegisterType<AppsMonthlyPersistenceMapper>().As<IAppsMonthlyPersistenceMapper>();

            var sqlFunc = new Func<SqlConnection>(() =>
                new SqlConnection(_dataPersistConfiguration.ReportDataConnectionString));

            builder.RegisterType<ReportDataPersistanceService<AppsMonthlyPayment>>()
                .WithParameter("sqlConnectionFunc", sqlFunc)
                .WithParameter(tableNameParameter, TableNameConstants.AppsMonthlyPayment)
                .As<IReportDataPersistanceService<AppsMonthlyPayment>>();

            RegisterDataProviders(builder);
        }

        private void RegisterDataProviders(ContainerBuilder builder)
        {
            builder.RegisterType<LearnerDataProvider>().As<ILearnerDataProvider>();

            builder.RegisterType<ILR2021_DataStoreEntities>().As<IIlr2021Context>();
            builder.Register(context =>
                {
                    var optionsBuilder = new DbContextOptionsBuilder<ILR2021_DataStoreEntities>();
                    optionsBuilder.UseSqlServer(_reportServiceConfiguration.ILRDataStoreConnectionString, options => options.EnableRetryOnFailure(3, TimeSpan.FromSeconds(3), new List<int>()));

                    return optionsBuilder.Options;
                }).As<DbContextOptions<ILR2021_DataStoreEntities>>()
                .SingleInstance();

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

            builder.Register(c =>
            {
                SqlConnection fcsSqlFunc() => new SqlConnection(_reportServiceConfiguration.FCSConnectionString);

                return new FcsDataProvider(fcsSqlFunc);
            }).As<IFcsDataProvider>();

            builder.Register(c =>
            {
                SqlConnection larsSqlFunc() => new SqlConnection(_reportServiceConfiguration.LarsConnectionString);

                var jsonSerializationService = c.Resolve<IJsonSerializationService>();

                return new LarsLearningDeliveryProvider(larsSqlFunc, jsonSerializationService);
            }).As<ILarsLearningDeliveryProvider>();
        }
    }
}