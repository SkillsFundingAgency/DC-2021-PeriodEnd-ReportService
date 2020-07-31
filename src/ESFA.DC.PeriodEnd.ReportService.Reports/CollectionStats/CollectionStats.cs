using System.Threading;
using System.Threading.Tasks;
using ESFA.DC.FileService.Interface;
using ESFA.DC.PeriodEnd.ReportService.Reports.Interface;
using ESFA.DC.PeriodEnd.ReportService.Reports.Interface.CollectionStats;
using ESFA.DC.PeriodEnd.ReportService.Reports.Interface.Enums;
using ESFA.DC.Serialization.Interfaces;

namespace ESFA.DC.PeriodEnd.ReportService.Reports.CollectionStats
{
    public class CollectionStats : IReport
    {
        private readonly IFileNameService _fileNameService;
        private readonly ICollectionStatsModelBuilder _modelBuilder;
        private readonly IFileService _fileService;
        private readonly IJsonSerializationService _jsonSerializationService;
        private string ReportFileName = "CollectionStats";

        public string ReportTaskName => "TaskGenerateCollectionStatsReport";
        public bool IncludeInZip => false;

        public CollectionStats(IFileNameService fileNameService, ICollectionStatsModelBuilder modelBuilder, IFileService fileService, IJsonSerializationService jsonSerializationService)
        {
            _fileNameService = fileNameService;
            _fileService = fileService;
            _modelBuilder = modelBuilder;
            _jsonSerializationService = jsonSerializationService;
        }

        public async Task<string> GenerateReport(IReportServiceContext reportServiceContext, CancellationToken cancellationToken)
        {
            var fileName = _fileNameService.GetInternalFilename(reportServiceContext, ReportFileName, OutputTypes.Json, false, false);

            var models = await _modelBuilder.BuildAsync(reportServiceContext.CollectionYear, reportServiceContext.ReturnPeriod);

            using (var stream = await _fileService.OpenWriteStreamAsync(fileName, reportServiceContext.Container, cancellationToken))
            {
                _jsonSerializationService.Serialize(models, stream);
            }
                
            return fileName;
        }
    }
}
