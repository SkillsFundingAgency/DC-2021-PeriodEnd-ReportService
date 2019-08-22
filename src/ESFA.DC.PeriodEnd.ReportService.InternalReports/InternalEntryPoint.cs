using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using ESFA.DC.Logging.Interfaces;
using ESFA.DC.PeriodEnd.ReportService.Interface;
using ESFA.DC.PeriodEnd.ReportService.Interface.Reports;

namespace ESFA.DC.PeriodEnd.ReportService.InternalReports
{
    public class InternalEntryPoint
    {
        private readonly ILogger _logger;
        private readonly IReportServiceContext _reportServiceContext;
        private readonly IEnumerable<IInternalReport> _internalReports;

        public InternalEntryPoint(
            ILogger logger,
            IReportServiceContext reportServiceContext,
            IEnumerable<IInternalReport> internalReports)
        {
            _logger = logger;
            _reportServiceContext = reportServiceContext;
            _internalReports = internalReports;
        }

        public async Task<bool> Callback(CancellationToken cancellationToken)
        {
            _logger.LogInfo("In internal entry point");

            try
            {
                foreach (string taskItem in _reportServiceContext.Tasks)
                {
                    if (cancellationToken.IsCancellationRequested)
                    {
                        break;
                    }

                    await GenerateReportAsync(taskItem, _reportServiceContext, cancellationToken);
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message, e);
                throw;
            }

            _logger.LogInfo("End of internal entry point");
            return true;
        }

        private async Task GenerateReportAsync(string task, IReportServiceContext reportServiceContext, CancellationToken cancellationToken)
        {
            var foundReport = false;
            foreach (var report in _internalReports)
            {
                if (!report.IsMatch(task))
                {
                    continue;
                }

                var stopWatch = new Stopwatch();
                stopWatch.Start();
                _logger.LogDebug($"Attempting to generate internal report {report.GetType().Name}");
                await report.GenerateReport(reportServiceContext, cancellationToken);
                stopWatch.Stop();
                _logger.LogDebug($"Persisted {report.GetType().Name} to csv/json/xlsx in: {stopWatch.ElapsedMilliseconds}");

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