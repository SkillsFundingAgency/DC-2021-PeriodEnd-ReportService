﻿using System;
using System.Collections.Generic;
using System.Linq;
using ESFA.DC.Logging.Interfaces;
using ESFA.DC.PeriodEnd.ReportService.Interface;
using ESFA.DC.PeriodEnd.ReportService.Interface.Reports;
using ESFA.DC.PeriodEnd.ReportService.Interface.Service;

namespace ESFA.DC.PeriodEnd.ReportService.Legacy.Service
{
    public class ReportsProvider : IReportsProvider
    {
        private readonly IList<ILegacyReport> _reports;
        private readonly ILogger _logger;

        public ReportsProvider(IList<ILegacyReport> reports, ILogger logger)
        {
            _reports = reports;
            _logger = logger;
        }

        public IEnumerable<ILegacyReport> ProvideReportsForContext(IReportServiceContext reportServiceContext)
        {
            var missingReportTasks =
                reportServiceContext
                    .Tasks
                    .Where(t => !_reports.Select(r => r.ReportTaskName).Contains(t, StringComparer.OrdinalIgnoreCase));

            foreach (var missingReportTask in missingReportTasks)
            {
                _logger.LogWarning($"Missing Report Task - {missingReportTask}");
            }

            return _reports.Where(r => reportServiceContext.Tasks.Contains(r.ReportTaskName, StringComparer.OrdinalIgnoreCase));
        }
    }
}
