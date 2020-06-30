using System;
using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;
using ESFA.DC.Logging.Interfaces;
using ESFA.DC.PeriodEnd.ReportService.Reports.Interface;
using ESFA.DC.PeriodEnd.ReportService.Reports.Interface.Enums;

namespace ESFA.DC.PeriodEnd.ReportService.Reports
{
    public class EntryPoint
    {
        private readonly IImmutableDictionary<string, IReport> _reports;
        private readonly IReportServiceContext _reportServiceContext;
        private readonly IFileNameService _fileNameService;
        private readonly ILogger _logger;

        private const string ReportsZipName = "Reports";

        public EntryPoint(
            IImmutableDictionary<string, IReport> reports,
            IReportServiceContext reportServiceContext,
            IFileNameService fileNameService,
            ILogger logger)
        {
            _reports = reports;
            _reportServiceContext = reportServiceContext;
            _fileNameService = fileNameService;
            _logger = logger;
        }

        public async Task<bool> Callback(CancellationToken cancellationToken)
        {
            _logger.LogInfo("Reporting callback invoked");

            var reportZipFileKey = _fileNameService.GetFilename(_reportServiceContext, ReportsZipName, OutputTypes.Zip, false);

            try
            {
                foreach (string taskItem in _reportServiceContext.Tasks)
                {
                    _reports.TryGetValue(taskItem, out var report);

                    if (cancellationToken.IsCancellationRequested)
                    {
                        break;
                    }

                    if (report == null)
                    {
                        throw new Exception($"Report with key {taskItem} not found");
                    }

                    var fileName = await report.GenerateReport(_reportServiceContext, cancellationToken);
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message, e);
                throw;
            }

            _logger.LogInfo("End of Reporting Entry Point");
            return true;
        }
    }
}
