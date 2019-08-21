using System;
using System.Collections.Generic;
using System.Text;

namespace ESFA.DC.PeriodEnd.ReportService.Interface
{
    public static class ReportTaskNameConstants
    {
        public const string AppsMonthlyPaymentReport = "TaskGenerateAppsMonthlyPaymentReport";
        public const string AppsAdditionalPaymentsReport = "TaskGenerateAppsAdditionalPaymentsReport";

        public static class InternalReports
        {
            public const string DataExtractReport = "TaskGenerateDataExtractReport";
            public const string PeriodEndMetricsReport = "PeriodEndMetrics";

            public static IEnumerable<string> TasksList = new List<string>()
            {
                DataExtractReport,
                PeriodEndMetricsReport
            };
        }
    }
}
