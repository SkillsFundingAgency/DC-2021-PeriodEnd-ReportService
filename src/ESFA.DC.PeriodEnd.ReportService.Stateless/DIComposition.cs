using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autofac;
using Autofac.Features.AttributeFilters;
using ESFA.DC.FileService;
using ESFA.DC.FileService.Config;
using ESFA.DC.FileService.Config.Interface;
using ESFA.DC.FileService.Interface;
using ESFA.DC.ILR1819.DataStore.EF;
using ESFA.DC.ILR1819.DataStore.EF.Interface;
using ESFA.DC.ILR1819.DataStore.EF.Valid;
using ESFA.DC.ILR1819.DataStore.EF.Valid.Interface;
using ESFA.DC.IO.AzureStorage;
using ESFA.DC.IO.AzureStorage.Config.Interfaces;
using ESFA.DC.IO.Interfaces;
using ESFA.DC.JobContextManager;
using ESFA.DC.JobContextManager.Interface;
using ESFA.DC.JobContextManager.Model;
using ESFA.DC.Mapping.Interface;
using ESFA.DC.PeriodEnd.ReportService.Interface.Configuration;
using ESFA.DC.PeriodEnd.ReportService.Interface.Service;
using ESFA.DC.PeriodEnd.ReportService.Service;
using ESFA.DC.PeriodEnd.ReportService.Service.Service;
using ESFA.DC.PeriodEnd.ReportService.Stateless.Configuration;
using ESFA.DC.PeriodEnd.ReportService.Stateless.Handlers;
using ESFA.DC.ServiceFabric.Common.Modules;
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

            containerBuilder.RegisterType<EntryPoint>().WithAttributeFiltering().InstancePerLifetimeScope();
            containerBuilder.RegisterType<ZipService>().As<IZipService>().InstancePerLifetimeScope();
            containerBuilder.RegisterType<ReportsProvider>().As<IReportsProvider>().InstancePerLifetimeScope();

            //containerBuilder.RegisterType<ILR1819_DataStoreEntitiesValid>().As<IIlr1819ValidContext>();
            //containerBuilder.Register(context =>
            //{
            //    var optionsBuilder = new DbContextOptionsBuilder<ILR1819_DataStoreEntitiesValid>();
            //    optionsBuilder.UseSqlServer(
            //        reportServiceConfiguration.ILRDataStoreValidConnectionString,
            //        options => options.EnableRetryOnFailure(3, TimeSpan.FromSeconds(3), new List<int>()));

            //        return optionsBuilder.Options;
            //    })
            //    .As<DbContextOptions<ILR1819_DataStoreEntitiesValid>>()
            //    .SingleInstance();

            //containerBuilder.RegisterType<ILR1819_DataStoreEntities>().As<IIlr1819RulebaseContext>();
            //containerBuilder.Register(context =>
            //{
            //    var optionsBuilder = new DbContextOptionsBuilder<ILR1819_DataStoreEntities>();
            //    optionsBuilder.UseSqlServer(
            //        reportServiceConfiguration.ILRDataStoreConnectionString,
            //        options => options.EnableRetryOnFailure(3, TimeSpan.FromSeconds(3), new List<int>()));

            //    return optionsBuilder.Options;
            //})
            //    .As<DbContextOptions<ILR1819_DataStoreEntities>>()
            //    .SingleInstance();

            //containerBuilder.RegisterType<DASPaymentsContext>().As<IDASPaymentsContext>();
            //containerBuilder.Register(context =>
            //{
            //    var optionsBuilder = new DbContextOptionsBuilder<DASPaymentsContext>();
            //    optionsBuilder.UseSqlServer(
            //        reportServiceConfiguration.DASPaymentsConnectionString,
            //        options => options.EnableRetryOnFailure(3, TimeSpan.FromSeconds(3), new List<int>()));

            //    return optionsBuilder.Options;
            //})
            //    .As<DbContextOptions<DASPaymentsContext>>()
            //    .SingleInstance();

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

            //containerBuilder.RegisterType<DateTimeProvider.DateTimeProvider>().As<IDateTimeProvider>().InstancePerLifetimeScope();

            //RegisterReports(containerBuilder);
            //RegisterServices(containerBuilder);
            //RegisterBuilders(containerBuilder);
            //RegisterRules(containerBuilder);
            //RegisterCommands(containerBuilder);

            return containerBuilder;
        }
    }
}
