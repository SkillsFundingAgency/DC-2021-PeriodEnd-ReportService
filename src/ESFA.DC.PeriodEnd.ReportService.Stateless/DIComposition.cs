using System;
using System.Collections.Generic;
using Autofac;
using Autofac.Features.AttributeFilters;
using ESFA.DC.DASPayments.EF;
using ESFA.DC.DASPayments.EF.Interfaces;
using ESFA.DC.DateTimeProvider.Interface;
using ESFA.DC.FileService;
using ESFA.DC.FileService.Config;
using ESFA.DC.FileService.Config.Interface;
using ESFA.DC.FileService.Interface;
using ESFA.DC.ILR1920.DataStore.EF;
using ESFA.DC.ILR1920.DataStore.EF.Interface;
using ESFA.DC.ILR1920.DataStore.EF.Valid;
using ESFA.DC.ILR1920.DataStore.EF.Valid.Interface;
using ESFA.DC.IO.AzureStorage;
using ESFA.DC.IO.AzureStorage.Config.Interfaces;
using ESFA.DC.IO.Interfaces;
using ESFA.DC.JobContextManager;
using ESFA.DC.JobContextManager.Interface;
using ESFA.DC.JobContextManager.Model;
using ESFA.DC.JobContextManager.Model.Interface;
using ESFA.DC.Mapping.Interface;
using ESFA.DC.PeriodEnd.ReportService.Interface;
using ESFA.DC.PeriodEnd.ReportService.Interface.Builders;
using ESFA.DC.PeriodEnd.ReportService.Interface.Builders.PeriodEnd;
using ESFA.DC.PeriodEnd.ReportService.Interface.Configuration;
using ESFA.DC.PeriodEnd.ReportService.Interface.Provider;
using ESFA.DC.PeriodEnd.ReportService.Interface.Reports;
using ESFA.DC.PeriodEnd.ReportService.Interface.Service;
using ESFA.DC.PeriodEnd.ReportService.Service;
using ESFA.DC.PeriodEnd.ReportService.Service.Builders;
using ESFA.DC.PeriodEnd.ReportService.Service.Builders.PeriodEnd;
using ESFA.DC.PeriodEnd.ReportService.Service.Provider;
using ESFA.DC.PeriodEnd.ReportService.Service.Reports;
using ESFA.DC.PeriodEnd.ReportService.Service.Service;
using ESFA.DC.PeriodEnd.ReportService.Stateless.Configuration;
using ESFA.DC.PeriodEnd.ReportService.Stateless.Context;
using ESFA.DC.PeriodEnd.ReportService.Stateless.Handlers;
using ESFA.DC.ReferenceData.LARS.Model;
using ESFA.DC.ReferenceData.LARS.Model.Interface;
using ESFA.DC.ServiceFabric.Common.Modules;
using Microsoft.EntityFrameworkCore;
using IValueProvider = System.Web.ModelBinding.IValueProvider;
using VersionInfo = ESFA.DC.PeriodEnd.ReportService.Stateless.Configuration.VersionInfo;

namespace ESFA.DC.PeriodEnd.ReportService.Stateless
{
    public static class DIComposition
    {
        public static ContainerBuilder BuildContainer(ServiceFabric.Common.Config.Interface.IServiceFabricConfigurationService serviceFabricConfigurationService)
        {
            var containerBuilder = new ContainerBuilder();

            var statelessServiceConfiguration = serviceFabricConfigurationService.GetConfigSectionAsStatelessServiceConfiguration();

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

            containerBuilder.RegisterType<EntryPoint>().WithAttributeFiltering().InstancePerLifetimeScope();
            containerBuilder.RegisterType<ZipService>().As<IZipService>().InstancePerLifetimeScope();
            containerBuilder.RegisterType<ReportsProvider>().As<IReportsProvider>().InstancePerLifetimeScope();

            containerBuilder.RegisterType<ILR1920_DataStoreEntities>().As<IIlr1920RulebaseContext>();
            containerBuilder.Register(context =>
            {
                var optionsBuilder = new DbContextOptionsBuilder<ILR1920_DataStoreEntities>();
                optionsBuilder.UseSqlServer(
                    reportServiceConfiguration.ILRDataStoreConnectionString,
                    options => options.EnableRetryOnFailure(3, TimeSpan.FromSeconds(3), new List<int>()));

                return optionsBuilder.Options;
            })
                .As<DbContextOptions<ILR1920_DataStoreEntities>>()
                .SingleInstance();

            containerBuilder.RegisterType<ILR1920_DataStoreEntitiesValid>().As<IIlr1920ValidContext>();
            containerBuilder.Register(context =>
            {
                var optionsBuilder = new DbContextOptionsBuilder<ILR1920_DataStoreEntitiesValid>();
                optionsBuilder.UseSqlServer(
                    reportServiceConfiguration.ILRDataStoreValidConnectionString,
                    options => options.EnableRetryOnFailure(3, TimeSpan.FromSeconds(3), new List<int>()));

                return optionsBuilder.Options;
            })
                .As<DbContextOptions<ILR1920_DataStoreEntitiesValid>>()
                .SingleInstance();

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

            //containerBuilder.RegisterType<FcsContext>().As<IFcsContext>();
            //containerBuilder.Register(context =>
            //{
            //    var optionsBuilder = new DbContextOptionsBuilder<FcsContext>();
            //    optionsBuilder.UseSqlServer(
            //        reportServiceConfiguration.FCSConnectionString,
            //        options => options.EnableRetryOnFailure(3, TimeSpan.FromSeconds(3), new List<int>()));

            //    return optionsBuilder.Options;
            //})
            //    .As<DbContextOptions<FcsContext>>()
            //    .SingleInstance();

            RegisterUtilities(containerBuilder);
            RegisterServices(containerBuilder);
            RegisterBuilders(containerBuilder);
            RegisterReports(containerBuilder);

            return containerBuilder;
        }

        private static void RegisterReports(ContainerBuilder containerBuilder)
        {
            containerBuilder.RegisterType<AppsMonthlyPaymentReport>().As<IReport>()
                .WithAttributeFiltering()
                .InstancePerLifetimeScope();

            containerBuilder.RegisterType<AppsAdditionalPaymentsReport>().As<IReport>()
                .WithAttributeFiltering()
                .InstancePerLifetimeScope();

            containerBuilder.Register(c => new List<IReport>(c.Resolve<IEnumerable<IReport>>()))
                .As<IList<IReport>>();
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

            containerBuilder.RegisterType<ReportServiceContext>().As<IReportServiceContext>()
                .InstancePerLifetimeScope();
        }

        private static void RegisterBuilders(ContainerBuilder containerBuilder)
        {
            containerBuilder.RegisterType<AppsMonthlyPaymentModelBuilder>().As<IAppsMonthlyPaymentModelBuilder>()
                .InstancePerLifetimeScope();

            containerBuilder.RegisterType<AppsAdditionalPaymentsModelBuilder>().As<IAppsAdditionalPaymentsModelBuilder>()
                .InstancePerLifetimeScope();
        }

        private static void RegisterUtilities(ContainerBuilder containerBuilder)
        {
            containerBuilder.RegisterType<DateTimeProvider.DateTimeProvider>().As<IDateTimeProvider>()
                .InstancePerLifetimeScope();
        }
    }
}
