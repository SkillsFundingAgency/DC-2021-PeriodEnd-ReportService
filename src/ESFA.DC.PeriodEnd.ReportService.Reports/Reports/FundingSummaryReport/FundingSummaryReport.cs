using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ESFA.DC.Logging.Interfaces;
using ESFA.DC.PeriodEnd.ReportService.Interface;
using ESFA.DC.PeriodEnd.ReportService.Interface.Reports;

namespace ESFA.DC.PeriodEnd.ReportService.Reports.Reports.FundingSummaryReport
{
    public class FundingSummaryReport : IReport
    {
        public IEnumerable<Type> DependsOn { get; }
        private readonly ILogger _logger;

        public string ReportTaskName => "TaskGenerateFundingSummaryReport";

        public FundingSummaryReport(ILogger logger)
        {
            _logger = logger;
        }

        public async Task GenerateReport(IReportServiceContext reportServiceContext, CancellationToken cancellationToken)
        {
        }
    }
}
