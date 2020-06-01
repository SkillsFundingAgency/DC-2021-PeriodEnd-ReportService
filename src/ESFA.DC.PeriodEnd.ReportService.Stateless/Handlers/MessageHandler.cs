using System;
using System.Fabric;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using ESFA.DC.IO.AzureStorage.Config.Interfaces;
using ESFA.DC.JobContext.Interface;
using ESFA.DC.JobContextManager.Interface;
using ESFA.DC.JobContextManager.Model;
using ESFA.DC.JobContextManager.Model.Interface;
using ESFA.DC.Logging.Interfaces;
using ESFA.DC.PeriodEnd.ReportService.Interface;
using ESFA.DC.PeriodEnd.ReportService.Interface.Configuration;
using ESFA.DC.PeriodEnd.ReportService.InternalReports;
using ESFA.DC.PeriodEnd.ReportService.Legacy;
using ESFA.DC.PeriodEnd.ReportService.Legacy.Constants;
using ESFA.DC.PeriodEnd.ReportService.Reports;
using ESFA.DC.PeriodEnd.ReportService.Stateless.Configuration;
using ESFA.DC.PeriodEnd.ReportService.Stateless.Context;
using ExecutionContext = ESFA.DC.Logging.ExecutionContext;

namespace ESFA.DC.PeriodEnd.ReportService.Stateless.Handlers
{
    public sealed class MessageHandler : IMessageHandler<JobContextMessage>
    {
        private readonly ILifetimeScope _parentLifeTimeScope;
        private readonly StatelessServiceContext _context;

        public MessageHandler(
            ILifetimeScope parentLifeTimeScope)
            : this(parentLifeTimeScope, null)
        {
        }

        public MessageHandler(
            ILifetimeScope parentLifeTimeScope,
            StatelessServiceContext context)
        {
            _parentLifeTimeScope = parentLifeTimeScope;
            _context = context;
        }

        public async Task<bool> HandleAsync(JobContextMessage jobContextMessage, CancellationToken cancellationToken)
        {
            try
            {
                using (var childLifeTimeScope = GetChildLifeTimeScope(jobContextMessage))
                {
                    bool result;

                    var executionContext = (ExecutionContext)childLifeTimeScope.Resolve<IExecutionContext>();
                    executionContext.JobId = jobContextMessage.JobId.ToString();
                    var logger = childLifeTimeScope.Resolve<ILogger>();

                    logger.LogDebug("Started Report Service");

                    var reportContext = childLifeTimeScope.Resolve<IReportServiceContext>();
                    var task = reportContext.Tasks.First();

                    if (ReportTaskNameConstants.InternalReports.TasksList.Contains(task))
                    {
                        var internalEntryPoint = childLifeTimeScope.Resolve<InternalEntryPoint>();
                        result = await internalEntryPoint.Callback(cancellationToken);

                        return result;
                    }

                    var legacyEntryPoint = childLifeTimeScope.Resolve<LegacyEntryPoint>();
                    result = await legacyEntryPoint.Callback(cancellationToken);

                    var entryPoint = childLifeTimeScope.Resolve<EntryPoint>();
                    result = await entryPoint.Callback(cancellationToken);

                    logger.LogDebug($"Completed Report Service with result-{result}");
                    return result;
                }
            }
            catch (OutOfMemoryException oom)
            {
                Environment.FailFast("Report Service Out Of Memory", oom);
                throw;
            }
            catch (Exception ex)
            {
                ServiceEventSource.Current.ServiceMessage(_context, "Exception-{0}", ex.ToString());
                throw;
            }
        }

        public ILifetimeScope GetChildLifeTimeScope(JobContextMessage jobContextMessage)
        {
            return _parentLifeTimeScope.BeginLifetimeScope(c =>
            {
                c.RegisterInstance(jobContextMessage).As<IJobContextMessage>();
                c.RegisterType<ReportServiceContext>().As<IReportServiceContext>();

                c.RegisterType<LegacyEntryPoint>().InstancePerLifetimeScope();
                c.RegisterType<InternalEntryPoint>().InstancePerLifetimeScope();
                c.RegisterType<EntryPoint>().InstancePerLifetimeScope();

                var azureBlobStorageOptions = _parentLifeTimeScope.Resolve<IAzureStorageOptions>();
                c.RegisterInstance(new AzureStorageKeyValuePersistenceConfig(
                        azureBlobStorageOptions.AzureBlobConnectionString,
                        jobContextMessage.KeyValuePairs[JobContextMessageKey.Container].ToString()))
                    .As<IAzureStorageKeyValuePersistenceServiceConfig>();
            });
        }
    }
}
