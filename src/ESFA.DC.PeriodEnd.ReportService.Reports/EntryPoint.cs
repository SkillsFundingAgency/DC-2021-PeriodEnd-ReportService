using System;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ESFA.DC.Logging.Interfaces;
using ESFA.DC.PeriodEnd.ReportService.Reports.Extensions;
using ESFA.DC.PeriodEnd.ReportService.Reports.Interface;

namespace ESFA.DC.PeriodEnd.ReportService.Reports
{
    public class EntryPoint
    {
        private readonly IImmutableDictionary<string, IReport> _reports;
        private readonly IReportServiceContext _reportServiceContext;
        private readonly IReportZipService _reportZipService;
        private readonly ILogger _logger;

        public EntryPoint(
            IImmutableDictionary<string, IReport> reports,
            IReportServiceContext reportServiceContext,
            IReportZipService reportZipService,
            ILogger logger)
        {
            _reports = reports;
            _reportServiceContext = reportServiceContext;
            _reportZipService = reportZipService;
            _logger = logger;
        }

        public async Task<bool> Callback(CancellationToken cancellationToken)
        {
            _logger.LogInfo("Reporting callback invoked");

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
                        if (taskItem.CaseInsensitiveEquals("TaskClearPeriodEndDASZip"))
                        {
                            await _reportZipService.RemoveZipAsync(_reportServiceContext, cancellationToken);
                        }
                        else
                        {
                            _logger.LogError($"Report with key {taskItem} not found");
                        }

                        continue;
                    }

                    var fileName = await report.GenerateReport(_reportServiceContext, cancellationToken);

                    if (report.IncludeInZip)
                    {
                        await _reportZipService.CreateOrUpdateZipWithReportAsync(fileName, _reportServiceContext, cancellationToken);
                    }
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
