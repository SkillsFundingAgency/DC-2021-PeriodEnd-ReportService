using System;
using System.Collections.Generic;
using System.Text;

namespace ESFA.DC.PeriodEnd.ReportService.Interface
{
    public static class ReportTaskNameConstants
    {
        public const string TaskClearPeriodEndDASZip = "TaskClearPeriodEndDASZip";

        public const string AppsMonthlyPaymentReport = "TaskGenerateAppsMonthlyPaymentReport";
        public const string AppsAdditionalPaymentsReport = "TaskGenerateAppsAdditionalPaymentsReport";
        public const string FundingSummaryReport = "TaskGenerateFundingSummaryReport";
        public const string AppsCoInvestmentContributionsReport = "TaskAppsCoInvestmentContributionsReport";

        public static class InternalReports
        {
            public const string DataExtractReport = "TaskGenerateDataExtractReport";
            public const string DataQualityReport = "TaskGenerateDataQualityReport";
            public const string PeriodEndMetricsReport = "TaskGeneratePeriodEndMetricsReport";

            public static IEnumerable<string> TasksList = new List<string>()
            {
                DataExtractReport,
                DataQualityReport,
                PeriodEndMetricsReport
            };
        }
    }
}
