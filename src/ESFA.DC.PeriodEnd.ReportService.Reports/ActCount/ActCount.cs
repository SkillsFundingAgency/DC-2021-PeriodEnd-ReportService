using System.Threading;
using System.Threading.Tasks;
using ESFA.DC.CsvService.Interface;
using ESFA.DC.PeriodEnd.ReportService.Reports.Interface;
using ESFA.DC.PeriodEnd.ReportService.Reports.Interface.ActCount;
using ESFA.DC.PeriodEnd.ReportService.Reports.Interface.ActCount.Model;
using ESFA.DC.PeriodEnd.ReportService.Reports.Interface.Enums;

namespace ESFA.DC.PeriodEnd.ReportService.Reports.ActCount
{
    public class ActCount : IReport
    {
        private readonly IFileNameService _fileNameService;
        private readonly ICsvFileService _csvFileService;
        private readonly IActCountModelBuilder _actCountModelBuilder;

        private string ReportFileName = "ACT Count Report";

        public string ReportTaskName => "TaskGenerateActCountReport";
        public bool IncludeInZip => false;

        public ActCount(IFileNameService fileNameService, ICsvFileService csvFileService, IActCountModelBuilder actCountModelBuilder)
        {
            _fileNameService = fileNameService;
            _csvFileService = csvFileService;
            _actCountModelBuilder = actCountModelBuilder;
        }

        public async Task<string> GenerateReport(IReportServiceContext reportServiceContext, CancellationToken cancellationToken)
        {
            var fileName = _fileNameService.GetInternalFilename(reportServiceContext, ReportFileName, OutputTypes.Csv);

            var models = await _actCountModelBuilder.BuildAsync(cancellationToken);

            await _csvFileService.WriteAsync<ActCountModel, ActCountClassMap>(models, fileName, reportServiceContext.Container, cancellationToken);

            return fileName;
        }
    }
}
