using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ESFA.DC.IO.Interfaces;
using ESFA.DC.Logging.Interfaces;
using ESFA.DC.PeriodEnd.ReportService.Interface;
using ESFA.DC.PeriodEnd.ReportService.Interface.Reports;
using ESFA.DC.PeriodEnd.ReportService.Legacy.Constants;
using ESFA.DC.PeriodEnd.ReportService.Legacy.Extensions;

namespace ESFA.DC.PeriodEnd.ReportService.Legacy
{
    public sealed class LegacyEntryPoint
    {
        private readonly ILogger _logger;

        private readonly IStreamableKeyValuePersistenceService _streamableKeyValuePersistenceService;
        private readonly IReportServiceContext _reportServiceContext;

        private readonly IList<ILegacyReport> _reports;

        public LegacyEntryPoint(
            ILogger logger,
            IStreamableKeyValuePersistenceService streamableKeyValuePersistenceService,
            IReportServiceContext reportServiceContext,
            IList<ILegacyReport> reports)
        {
            _logger = logger;
            _streamableKeyValuePersistenceService = streamableKeyValuePersistenceService;
            _reportServiceContext = reportServiceContext;
            _reports = reports;
        }

        public async Task<bool> Callback(CancellationToken cancellationToken)
        {
            _logger.LogInfo("Reporting callback invoked");

            var reportZipFileKey = $"R{_reportServiceContext.ReturnPeriod:00}_{_reportServiceContext.Ukprn}_Reports.zip";
            cancellationToken.ThrowIfCancellationRequested();

            MemoryStream memoryStream = new MemoryStream();
            var zipFileExists = await _streamableKeyValuePersistenceService.ContainsAsync(reportZipFileKey, cancellationToken);
            if (zipFileExists)
            {
                if (_reportServiceContext.Tasks.Any(x => x.CaseInsensitiveEquals(ReportTaskNameConstants.TaskClearPeriodEndDASZip)))
                {
                    await _streamableKeyValuePersistenceService.RemoveAsync(reportZipFileKey, cancellationToken);
                }
                else
                {
                    await _streamableKeyValuePersistenceService.GetAsync(reportZipFileKey, memoryStream, cancellationToken);
                }
            }

            using (memoryStream)
            {
                using (var archive = new ZipArchive(memoryStream, ZipArchiveMode.Update, true))
                {
                    await ExecuteTasks(_reportServiceContext, archive, cancellationToken);
                }

                await _streamableKeyValuePersistenceService.SaveAsync(reportZipFileKey, memoryStream, cancellationToken);
            }

            return true;
        }

        private async Task ExecuteTasks(IReportServiceContext reportServiceContext, ZipArchive archive, CancellationToken cancellationToken)
        {
            foreach (string taskItem in reportServiceContext.Tasks)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    break;
                }

                await GenerateReportAsync(taskItem, reportServiceContext, archive, cancellationToken);
            }
        }

        private async Task GenerateReportAsync(string task, IReportServiceContext reportServiceContext, ZipArchive archive, CancellationToken cancellationToken)
        {
            var foundReport = false;
            foreach (var report in _reports)
            {
                if (!report.IsMatch(task))
                {
                    continue;
                }

                var stopWatch = new Stopwatch();
                stopWatch.Start();
                _logger.LogDebug($"Attempting to generate {report.GetType().Name}");
                await report.GenerateReport(reportServiceContext, archive, cancellationToken);
                stopWatch.Stop();
                _logger.LogDebug($"Persisted {report.GetType().Name} to csv/json in: {stopWatch.ElapsedMilliseconds}");

                foundReport = true;
                break;
            }

            if (!foundReport)
            {
                _logger.LogDebug($"Unable to find report '{task}'");
            }
        }
    }
}
