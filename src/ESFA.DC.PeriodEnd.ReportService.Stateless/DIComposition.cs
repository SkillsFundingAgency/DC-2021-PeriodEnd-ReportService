using System;
using System.Collections.Generic;
using Autofac;
using Autofac.Features.AttributeFilters;
using ESFA.DC.DASPayments.EF;
using ESFA.DC.DASPayments.EF.Interfaces;
using ESFA.DC.DateTimeProvider.Interface;
using ESFA.DC.EAS2021.EF;
using ESFA.DC.EAS2021.EF.Interface;
using ESFA.DC.FileService;
using ESFA.DC.FileService.Config;
using ESFA.DC.FileService.Config.Interface;
using ESFA.DC.FileService.Interface;
using ESFA.DC.ILR.ReferenceDataService.ILRReferenceData.Model;
using ESFA.DC.ILR.ReferenceDataService.ILRReferenceData.Model.Interface;
using ESFA.DC.ILR2021.DataStore.EF;
using ESFA.DC.ILR2021.DataStore.EF.Interface;
using ESFA.DC.ILR2021.DataStore.EF.Invalid;
using ESFA.DC.ILR2021.DataStore.EF.Invalid.Interface;
using ESFA.DC.ILR2021.DataStore.EF.StoredProc;
using ESFA.DC.IO.AzureStorage;
using ESFA.DC.IO.AzureStorage.Config.Interfaces;
using ESFA.DC.IO.Interfaces;
using ESFA.DC.JobContextManager;
using ESFA.DC.JobContextManager.Interface;
using ESFA.DC.JobContextManager.Model;
using ESFA.DC.JobContextManager.Model.Interface;
using ESFA.DC.JobQueueManager.Data;
using ESFA.DC.Mapping.Interface;
using ESFA.DC.PeriodEnd.DataPersist;
using ESFA.DC.PeriodEnd.ReportService.DataAccess.Contexts;
using ESFA.DC.PeriodEnd.ReportService.DataAccess.Services;
using ESFA.DC.PeriodEnd.ReportService.Interface;
using ESFA.DC.PeriodEnd.ReportService.Interface.Builders;
using ESFA.DC.PeriodEnd.ReportService.Interface.Configuration;
using ESFA.DC.PeriodEnd.ReportService.Interface.DataAccess;
using ESFA.DC.PeriodEnd.ReportService.Interface.Model.FundingSummaryReport;
using ESFA.DC.PeriodEnd.ReportService.Interface.Provider;
using ESFA.DC.PeriodEnd.ReportService.Interface.Reports;
using ESFA.DC.PeriodEnd.ReportService.Interface.Service;
using ESFA.DC.PeriodEnd.ReportService.InternalReports.Reports;
using ESFA.DC.PeriodEnd.ReportService.Legacy.Builders;
using ESFA.DC.PeriodEnd.ReportService.Legacy.Interface;
using ESFA.DC.PeriodEnd.ReportService.Legacy.Provider;
using ESFA.DC.PeriodEnd.ReportService.Legacy.Reports;
using ESFA.DC.PeriodEnd.ReportService.Legacy.Reports.AppsAdditionalPaymentsReport;
using ESFA.DC.PeriodEnd.ReportService.Legacy.Reports.FundingSummaryReport;
using ESFA.DC.PeriodEnd.ReportService.Legacy.Service;
using ESFA.DC.PeriodEnd.ReportService.Model.PeriodEnd.AppsCoInvestment;
using ESFA.DC.PeriodEnd.ReportService.Model.PeriodEnd.AppsCoInvestment.Comparer;
using ESFA.DC.PeriodEnd.ReportService.Model.PeriodEnd.LearnerLevelView;
using ESFA.DC.PeriodEnd.ReportService.Model.PeriodEnd.LearnerLevelView.Comparer;
using ESFA.DC.PeriodEnd.ReportService.Modules;
using ESFA.DC.PeriodEnd.ReportService.Stateless.Configuration;
using ESFA.DC.PeriodEnd.ReportService.Stateless.Context;
using ESFA.DC.PeriodEnd.ReportService.Stateless.Handlers;
using ESFA.DC.ReferenceData.FCS.Model;
using ESFA.DC.ReferenceData.FCS.Model.Interface;
using ESFA.DC.ReferenceData.LARS.Model;
using ESFA.DC.ReferenceData.LARS.Model.Interface;
using ESFA.DC.ReferenceData.Organisations.Model;
using ESFA.DC.ReferenceData.Organisations.Model.Interface;
using ESFA.DC.ServiceFabric.Common.Modules;
using ESFA.DC.Summarisation.Model;
using ESFA.DC.Summarisation.Model.Interface;
using Microsoft.EntityFrameworkCore;
using VersionInfo = ESFA.DC.PeriodEnd.ReportService.Stateless.Configuration.VersionInfo;

namespace ESFA.DC.PeriodEnd.ReportService.Stateless
{
    public static class DIComposition
    {
        public static ContainerBuilder BuildContainer(ServiceFabric.Common.Config.Interface.IServiceFabricConfigurationService serviceFabricConfigurationService)
        {
            var containerBuilder = new ContainerBuilder();

            var statelessServiceConfiguration = serviceFabricConfigurationService.GetConfigSectionAsStatelessServiceConfiguration();

            var dataPersistConfiguration = serviceFabricConfigurationService.GetConfigSectionAs<DataPersistConfiguration>("DataPersistConfiguration");
            containerBuilder.RegisterInstance(dataPersistConfiguration).As<DataPersistConfiguration>();

            var reportServiceConfiguration = serviceFabricConfigurationService.GetConfigSectionAs<ReportServiceConfiguration>("ReportServiceConfiguration");
            containerBuilder.RegisterInstance(reportServiceConfiguration).As<IReportServiceConfiguration>();

            // register azure blob storage service
            var azureBlobStorageOptions = serviceFabricConfigurationService.GetConfigSectionAs<AzureStorageOptions>("AzureStorageSection");
            containerBuilder.RegisterInstance(azureBlobStorageOptions).As<IAzureStorageOptions>();
            containerBuilder.Register(c =>
                    new AzureStorageKeyValuePersistenceConfig(
                        azureBlobStorageOptions.AzureBlobConnectionString,
                        azureBlobStorageOptions.AzureBlobContainerName))
                .As<IAzureStorageKeyValuePersistenceServiceConfig>().SingleInstance();

            containerBuilder.RegisterType<AzureStorageKeyValuePersistenceService>()
                .As<IStreamableKeyValuePersistenceService>()
                .InstancePerLifetimeScope();

            var azureStorageFileServiceConfiguration = new AzureStorageFileServiceConfiguration()
            {
                ConnectionString = azureBlobStorageOptions.AzureBlobConnectionString,
            };

            containerBuilder.RegisterInstance(azureStorageFileServiceConfiguration).As<IAzureStorageFileServiceConfiguration>();
            containerBuilder.RegisterType<AzureStorageFileService>().As<IFileService>();

            containerBuilder.RegisterModule(new StatelessServiceModule(statelessServiceConfiguration));
            containerBuilder.RegisterModule<SerializationModule>();

            var versionInfo = serviceFabricConfigurationService.GetConfigSectionAs<VersionInfo>("VersionSection");
            containerBuilder.RegisterInstance(versionInfo).As<IVersionInfo>().SingleInstance();

            // register message mapper
            containerBuilder.RegisterType<DefaultJobContextMessageMapper<JobContextMessage>>().As<IMapper<JobContextMessage, JobContextMessage>>();

            // register MessageHandler
            containerBuilder.RegisterType<MessageHandler>().As<IMessageHandler<JobContextMessage>>().InstancePerLifetimeScope();

            containerBuilder.RegisterType<JobContextManager<JobContextMessage>>().As<IJobContextManager<JobContextMessage>>()
                .InstancePerLifetimeScope();

            containerBuilder.RegisterType<JobContextMessage>().As<IJobContextMessage>()
                .InstancePerLifetimeScope();

            containerBuilder.RegisterType<ReportsProvider>().As<IReportsProvider>().InstancePerLifetimeScope();

            RegisterContexts(containerBuilder, reportServiceConfiguration);
            RegisterUtilities(containerBuilder);
            RegisterServices(containerBuilder);
            RegisterBuilders(containerBuilder);
            RegisterReports(containerBuilder);
            RegisterDataPersist(containerBuilder);

            RegisterFundingSummaryReport(containerBuilder);

            containerBuilder.RegisterModule<ServicesModule>();
            containerBuilder.RegisterModule(new ReportModule(reportServiceConfiguration, dataPersistConfiguration));

            return containerBuilder;
        }

        private static void RegisterContexts(ContainerBuilder containerBuilder, IReportServiceConfiguration reportServiceConfiguration)
        {
            // ILR 1920 DataStore
            containerBuilder.RegisterType<ILR2021_DataStoreEntities>().As<IIlr2021Context>();
            containerBuilder.Register(context =>
            {
                var optionsBuilder = new DbContextOptionsBuilder<ILR2021_DataStoreEntities>();
                optionsBuilder.UseSqlServer(
                    reportServiceConfiguration.ILRDataStoreConnectionString,
                    options => options.EnableRetryOnFailure(3, TimeSpan.FromSeconds(3), new List<int>()));

                return optionsBuilder.Options;
            })
                .As<DbContextOptions<ILR2021_DataStoreEntities>>()
                .SingleInstance();

            // ILR 1920 DataStore Valid Learners
            containerBuilder.RegisterType<ILR2021_DataStoreEntities>().As<IIlr2021Context>();
            containerBuilder.Register(context =>
            {
                var optionsBuilder = new DbContextOptionsBuilder<ILR2021_DataStoreEntities>();
                optionsBuilder.UseSqlServer(
                    reportServiceConfiguration.ILRDataStoreConnectionString,
                    options => options.EnableRetryOnFailure(3, TimeSpan.FromSeconds(3), new List<int>()));

                return optionsBuilder.Options;
            }).As<DbContextOptions<ILR2021_DataStoreEntities>>()
                .SingleInstance();

            containerBuilder.Register(context =>
            {
                var optionsBuilder = new DbContextOptionsBuilder<ILR2021_DataStoreEntities>();
                optionsBuilder.UseSqlServer(
                    reportServiceConfiguration.ILRDataStoreConnectionString,
                    options => options.EnableRetryOnFailure(3, TimeSpan.FromSeconds(3), new List<int>()));

                return new ILR2021_DataStoreEntities(optionsBuilder.Options);
            }).As<ILR2021_DataStoreEntities>()
                .ExternallyOwned();

            // ILR 1920 DataStore InValid Learners
            containerBuilder.RegisterType<ILR2021_DataStoreEntitiesInvalid>().As<IIlr2021InvalidContext>().ExternallyOwned();
            containerBuilder.Register(context =>
            {
                var optionsBuilder = new DbContextOptionsBuilder<ILR2021_DataStoreEntitiesInvalid>();
                optionsBuilder.UseSqlServer(
                    reportServiceConfiguration.ILRDataStoreConnectionString,
                    options => options.EnableRetryOnFailure(3, TimeSpan.FromSeconds(3), new List<int>()));

                return optionsBuilder.Options;
            })
                .As<DbContextOptions<ILR2021_DataStoreEntitiesInvalid>>()
                .SingleInstance();

            containerBuilder.Register(context =>
                {
                    var optionsBuilder = new DbContextOptionsBuilder<ILR2021_DataStoreEntitiesStoredProc>();
                    optionsBuilder.UseSqlServer(
                        reportServiceConfiguration.ILRDataStoreConnectionString,
                        options =>
                        {
                            options.EnableRetryOnFailure(3, TimeSpan.FromSeconds(3), new List<int>());
                            if (int.TryParse(reportServiceConfiguration.PeriodEndReportServiceDBCommandTimeout, out int commandTimeout) && commandTimeout > 0)
                            {
                                options.CommandTimeout(commandTimeout);
                            }
                        });

                    return new ILR2021_DataStoreEntitiesStoredProc(optionsBuilder.Options);
                }).As<ILR2021_DataStoreEntitiesStoredProc>()
                .ExternallyOwned();

            // Eas 1920
            containerBuilder.RegisterType<EasContext>().As<IEasdbContext>().ExternallyOwned();
            containerBuilder.Register(context =>
                {
                    var optionsBuilder = new DbContextOptionsBuilder<EasContext>();
                    optionsBuilder.UseSqlServer(
                        reportServiceConfiguration.EasConnectionString,
                        options => options.EnableRetryOnFailure(3, TimeSpan.FromSeconds(3), new List<int>()));

                    return optionsBuilder.Options;
                })
                .As<DbContextOptions<EasContext>>()
                .SingleInstance();

            // DAS Payments
            containerBuilder.RegisterType<DASPaymentsContext>().As<IDASPaymentsContext>();
            containerBuilder.Register(context =>
            {
                var optionsBuilder = new DbContextOptionsBuilder<DASPaymentsContext>();
                optionsBuilder.UseSqlServer(
                    reportServiceConfiguration.DASPaymentsConnectionString,
                    options => options.EnableRetryOnFailure(3, TimeSpan.FromSeconds(3), new List<int>()));

                return optionsBuilder.Options;
            })
                .As<DbContextOptions<DASPaymentsContext>>()
                .SingleInstance();

            containerBuilder.Register(c =>
            {
                var optionsBuilder = new DbContextOptionsBuilder<DASPaymentsContext>();
                optionsBuilder.UseSqlServer(
                    reportServiceConfiguration.DASPaymentsConnectionString,
                    providerOptions => providerOptions.CommandTimeout(60));
                return new PaymentsContext(optionsBuilder.Options);
            })
                .As<PaymentsContext>().ExternallyOwned();

            // LARS
            containerBuilder.RegisterType<LarsContext>().As<ILARSContext>().ExternallyOwned();
            containerBuilder.Register(context =>
            {
                var optionsBuilder = new DbContextOptionsBuilder<LarsContext>();
                optionsBuilder.UseSqlServer(
                    reportServiceConfiguration.LarsConnectionString,
                    options => options.EnableRetryOnFailure(3, TimeSpan.FromSeconds(3), new List<int>()));

                return optionsBuilder.Options;
            })
                .As<DbContextOptions<LarsContext>>()
                .SingleInstance();

            // FCS
            containerBuilder.RegisterType<FcsContext>().As<IFcsContext>();
            containerBuilder.Register(context =>
            {
                var optionsBuilder = new DbContextOptionsBuilder<FcsContext>();
                optionsBuilder.UseSqlServer(
                    reportServiceConfiguration.FCSConnectionString,
                    options => options.EnableRetryOnFailure(3, TimeSpan.FromSeconds(3), new List<int>()));

                return optionsBuilder.Options;
            })
                .As<DbContextOptions<FcsContext>>()
                .SingleInstance();

            // Summarisation
            containerBuilder.RegisterType<SummarisationContext>().As<ISummarisationContext>();
            containerBuilder.Register(context =>
            {
                var optionsBuilder = new DbContextOptionsBuilder<SummarisationContext>();
                optionsBuilder.UseSqlServer(
                    reportServiceConfiguration.SummarisedActualsConnectionString,
                    options => options.EnableRetryOnFailure(3, TimeSpan.FromSeconds(3), new List<int>()));

                return optionsBuilder.Options;
            })
                .As<DbContextOptions<SummarisationContext>>()
                .SingleInstance();

            // ILr Reference Data
            containerBuilder.RegisterType<IlrReferenceDataContext>().As<IIlrReferenceDataContext>().ExternallyOwned();
            containerBuilder.Register(context =>
                {
                    var optionsBuilder = new DbContextOptionsBuilder<IlrReferenceDataContext>();
                    optionsBuilder.UseSqlServer(
                        reportServiceConfiguration.ILRReferenceDataConnectionString,
                        options => options.EnableRetryOnFailure(3, TimeSpan.FromSeconds(3), new List<int>()));

                    return optionsBuilder.Options;
                })
                .As<DbContextOptions<IlrReferenceDataContext>>()
                .SingleInstance();

            // Organisation
            containerBuilder.RegisterType<OrganisationsContext>().As<IOrganisationsContext>().ExternallyOwned();
            containerBuilder.Register(context =>
            {
                var optionsBuilder = new DbContextOptionsBuilder<OrganisationsContext>();
                optionsBuilder.UseSqlServer(
                    reportServiceConfiguration.OrgConnectionString,
                    options => options.EnableRetryOnFailure(3, TimeSpan.FromSeconds(3), new List<int>()));

                return optionsBuilder.Options;
            })
                .As<DbContextOptions<OrganisationsContext>>()
                .SingleInstance();

            // Job Queue Manager
            containerBuilder.RegisterType<JobQueueDataContext>().As<IJobQueueDataContext>().ExternallyOwned();
            containerBuilder.Register(context =>
            {
                var optionsBuilder = new DbContextOptionsBuilder<JobQueueDataContext>();
                optionsBuilder.UseSqlServer(
                    reportServiceConfiguration.JobQueueManagerConnectionString,
                    options => options.EnableRetryOnFailure(3, TimeSpan.FromSeconds(3), new List<int>()));

                return optionsBuilder.Options;
            })
                .As<DbContextOptions<JobQueueDataContext>>()
                .SingleInstance();
        }

        private static void RegisterReports(ContainerBuilder containerBuilder)
        {
            containerBuilder.RegisterType<LearnerLevelViewReport>().As<ILegacyReport>()
                .WithAttributeFiltering()
                .InstancePerLifetimeScope();

            containerBuilder.RegisterType<AppsMonthlyPaymentReport>().As<ILegacyReport>()
                .WithAttributeFiltering()
                .InstancePerLifetimeScope();

            containerBuilder.RegisterType<AppsAdditionalPaymentsReport>().As<ILegacyReport>()
                .WithAttributeFiltering()
                .InstancePerLifetimeScope();

            containerBuilder.RegisterType<AppsCoInvestmentContributionsReport>().As<ILegacyReport>()
                .WithAttributeFiltering()
                .InstancePerLifetimeScope();

            containerBuilder.Register(c => new List<ILegacyReport>(c.Resolve<IEnumerable<ILegacyReport>>()))
                .As<IList<ILegacyReport>>();

            containerBuilder.RegisterType<PeriodEndMetricsReport>().As<IInternalReport>();

            containerBuilder.RegisterType<DataExtractReport>().As<IInternalReport>();

            containerBuilder.RegisterType<DataQualityReport>().As<IInternalReport>();

            containerBuilder.RegisterType<ProviderSubmissionsReport>().As<IInternalReport>();

            containerBuilder.RegisterType<CollectionStatsReport>().As<IInternalReport>();

            containerBuilder.RegisterType<ActCountReport>().As<IInternalReport>();
        }

        private static void RegisterDataPersist(ContainerBuilder containerBuilder)
        {
            containerBuilder.RegisterType<PersistReportData>().As<IPersistReportData>();
            containerBuilder.RegisterType<BulkInsert>().As<IBulkInsert>();
        }

        private static void RegisterServices(ContainerBuilder containerBuilder)
        {
            containerBuilder.RegisterType<IlrPeriodEndProviderService>().As<IIlrPeriodEndProviderService>()
                .WithAttributeFiltering()
                .InstancePerLifetimeScope();

            containerBuilder.RegisterType<DASPaymentsProviderService>().As<IDASPaymentsProviderService>()
                .InstancePerLifetimeScope();

            containerBuilder.RegisterType<LarsProviderService>().As<ILarsProviderService>()
                .InstancePerLifetimeScope();

            containerBuilder.RegisterType<FM36PeriodEndProviderService>().As<IFM36PeriodEndProviderService>()
                .WithAttributeFiltering()
                .InstancePerLifetimeScope();

            containerBuilder.RegisterType<ValueProvider>().As<IValueProvider>()
                .InstancePerLifetimeScope();

            containerBuilder.RegisterType<SummarisationProviderService>().As<ISummarisationProviderService>()
                .InstancePerLifetimeScope();

            containerBuilder.RegisterType<FCSProviderService>().As<IFCSProviderService>()
                .InstancePerLifetimeScope();

            containerBuilder.RegisterType<ReportServiceContext>().As<IReportServiceContext>().As<Reports.Interface.IReportServiceContext>()
                .InstancePerLifetimeScope();

            containerBuilder.RegisterType<PaymentsService>().As<IPaymentsService>()
                .InstancePerLifetimeScope();

            containerBuilder.RegisterType<PeriodEndQueryService>().As<IPeriodEndQueryService1920>()
                .InstancePerLifetimeScope();

            containerBuilder.RegisterType<OrgProviderService>().As<IOrgProviderService>()
                .InstancePerLifetimeScope();

            containerBuilder.RegisterType<JobQueueManagerProviderService>().As<IJobQueueManagerProviderService>()
                .InstancePerLifetimeScope();

            containerBuilder.RegisterType<ReferenceDataService>().As<IReferenceDataService>()
                .InstancePerLifetimeScope();
        }

        private static void RegisterBuilders(ContainerBuilder containerBuilder)
        {
            containerBuilder.RegisterType<LearnerLevelViewModelBuilder>().As<ILearnerLevelViewModelBuilder>()
                .InstancePerLifetimeScope();

            containerBuilder.RegisterType<AppsMonthlyPaymentModelBuilder>().As<IAppsMonthlyPaymentModelBuilder>()
                .InstancePerLifetimeScope();

            containerBuilder.RegisterType<AppsAdditionalPaymentsModelBuilder>().As<IAppsAdditionalPaymentsModelBuilder>()
                .InstancePerLifetimeScope();

            containerBuilder.RegisterType<AppsCoInvestmentContributionsModelBuilder>().As<IAppsCoInvestmentContributionsModelBuilder>()
                .InstancePerLifetimeScope();

            containerBuilder.RegisterType<DataExtractModelBuilder>().As<IDataExtractModelBuilder>()
                .InstancePerLifetimeScope();

            containerBuilder.RegisterType<ProviderSubmissionsModelBuilder>().As<IProviderSubmissionsModelBuilder>()
                .InstancePerLifetimeScope();
        }

        private static void RegisterUtilities(ContainerBuilder containerBuilder)
        {
            containerBuilder.RegisterType<DateTimeProvider.DateTimeProvider>().As<IDateTimeProvider>()
                .InstancePerLifetimeScope();

            containerBuilder.RegisterType<Legacy.Service.ExcelService>().As<IExcelService>().InstancePerLifetimeScope();

            containerBuilder.RegisterType<LLVPaymentRecordKeyEqualityComparer>().As<ILLVPaymentRecordKeyEqualityComparer>().InstancePerLifetimeScope();

            containerBuilder.RegisterType<LLVPaymentRecordLRefOnlyKeyEqualityComparer>().As<ILLVPaymentRecordLRefOnlyKeyEqualityComparer>().InstancePerLifetimeScope();

            containerBuilder.RegisterType<AppsCoInvestmentRecordKeyEqualityComparer>().As<IEqualityComparer<AppsCoInvestmentRecordKey>>().InstancePerLifetimeScope();
        }

        private static void RegisterFundingSummaryReport(ContainerBuilder containerBuilder)
        {
            containerBuilder.RegisterType<FundingSummaryReport>().As<ILegacyReport>().InstancePerLifetimeScope();
            containerBuilder.RegisterType<FundingSummaryReportModelBuilder>().As<IFundingSummaryReportModelBuilder>().InstancePerLifetimeScope();
            containerBuilder.RegisterType<FundingSummaryReportRenderService>().As<IRenderService<IFundingSummaryReport>>().InstancePerLifetimeScope();
            containerBuilder.RegisterType<PeriodisedValuesLookupProviderService>().As<IPeriodisedValuesLookupProviderService>().InstancePerLifetimeScope();
        }
    }
}
