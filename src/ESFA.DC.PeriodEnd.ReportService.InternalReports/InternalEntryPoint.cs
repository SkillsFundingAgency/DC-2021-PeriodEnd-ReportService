﻿using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ESFA.DC.Logging.Interfaces;
using ESFA.DC.PeriodEnd.ReportService.Interface.Reports;

namespace ESFA.DC.PeriodEnd.ReportService.InternalReports
{
    public class InternalEntryPoint
    {
        private readonly ILogger _logger;
        private readonly IEnumerable<IInternalReport> _internalReports;

        public InternalEntryPoint(
            ILogger logger,
            IEnumerable<IInternalReport> internalReports)
        {
            _logger = logger;
            _internalReports = internalReports;
        }

        public async Task<bool> Callback(CancellationToken cancellationToken)
        {
            _logger.LogInfo("In internal entry point");

            try
            {
                foreach (var report in _internalReports)
                {
                    await report.GenerateReport(cancellationToken);
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
    }
}