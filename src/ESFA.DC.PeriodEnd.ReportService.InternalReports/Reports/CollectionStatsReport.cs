using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ESFA.DC.DateTimeProvider.Interface;
using ESFA.DC.IO.Interfaces;
using ESFA.DC.PeriodEnd.ReportService.Interface;
using ESFA.DC.PeriodEnd.ReportService.Interface.Provider;
using ESFA.DC.PeriodEnd.ReportService.Interface.Reports;
using ESFA.DC.PeriodEnd.ReportService.Interface.Service;
using ESFA.DC.PeriodEnd.ReportService.Model.ReportModels;
using ESFA.DC.Serialization.Interfaces;

namespace ESFA.DC.PeriodEnd.ReportService.InternalReports.Reports
{
    public sealed class CollectionStatsReport : AbstractInternalReport, IInternalReport
    {
        private readonly IStreamableKeyValuePersistenceService _streamableKeyValuePersistenceService;
        private readonly IJobQueueManagerProviderService _jobQueueManagerProviderService;
        private readonly IJsonSerializationService _jsonSerializationService;

        public CollectionStatsReport(
            IDateTimeProvider dateTimeProvider,
            IValueProvider valueProvider,
            IJsonSerializationService jsonSerializationService,
            IJobQueueManagerProviderService jobQueueManagerProviderService,
            IStreamableKeyValuePersistenceService streamableKeyValuePersistenceService)
            : base(valueProvider, dateTimeProvider)
        {
            _jsonSerializationService = jsonSerializationService;
            _jobQueueManagerProviderService = jobQueueManagerProviderService;
            _streamableKeyValuePersistenceService = streamableKeyValuePersistenceService;
        }

        public override string ReportFileName { get; set; } = "Collection Stats Report";

        public override string ReportTaskName => ReportTaskNameConstants.InternalReports.CollectionStatsReport;

        public override async Task GenerateReport(IReportServiceContext reportServiceContext, CancellationToken cancellationToken)
        {
            var externalFileName = $"R{reportServiceContext.ReturnPeriod:D2}_CollectionStats";

            IEnumerable<CollectionStatsModel> collectionStatsInfo = (await _jobQueueManagerProviderService.GetCollectionStatsModels(
                reportServiceContext.CollectionYear, reportServiceContext.ReturnPeriod, cancellationToken)).ToList();

            string json = await GetJson(collectionStatsInfo, cancellationToken);
            await _streamableKeyValuePersistenceService.SaveAsync($"{externalFileName}.json", json, cancellationToken);
        }

        private async Task<string> GetJson(
            IEnumerable<CollectionStatsModel> collectionStatsModel,
            CancellationToken cancellationToken)
        {
            return _jsonSerializationService.Serialize<IEnumerable<CollectionStatsModel>>(collectionStatsModel);
        }
    }
}
