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
        public const string AppsCoInvestmentContributionsReport = "TaskGenerateAppsCoInvestmentContributionsReport";
        public const string LearnerLevelViewReport = "TaskLearnerLevelViewReport";

        public static class InternalReports
        {
            public const string CollectionStatsReport = "TaskGenerateCollectionStatsReport";
            public const string DataExtractReport = "TaskGenerateDataExtractReport";
            public const string DataQualityReport = "TaskGenerateDataQualityReport";
            public const string PeriodEndMetricsReport = "TaskGeneratePeriodEndMetricsReport";
            public const string ProviderSubmissionsReport = "TaskGenerateProviderSubmissionsReport";

            public static IEnumerable<string> TasksList = new List<string>()
            {
                CollectionStatsReport,
                DataExtractReport,
                DataQualityReport,
                PeriodEndMetricsReport,
                ProviderSubmissionsReport
            };
        }
    }
}
