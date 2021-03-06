﻿using System.Collections.Generic;

namespace ESFA.DC.PeriodEnd.ReportService.Legacy.Constants
{
    public static class ReportTaskNameConstants
    {
        public const string TaskClearPeriodEndDASZip = "TaskClearPeriodEndDASZip";

        public const string AppsMonthlyPaymentReport = "TaskGenerateAppsMonthlyPaymentReport";
        public const string AppsAdditionalPaymentsReport = "TaskGenerateAppsAdditionalPaymentsReport";
        public const string FundingSummaryReport = "TaskGenerateFundingSummaryPeriodEndReport";
        public const string AppsCoInvestmentContributionsReport = "TaskGenerateAppsCoInvestmentContributionsReport";
        public const string LearnerLevelViewReport = "TaskLearnerLevelViewReport";

        public const string IlrCollectionName = "ILR";

        public static class InternalReports
        {
            public const string CollectionStatsReport = "TaskGenerateCollectionStatsReport";
            public const string DataExtractReport = "TaskGenerateDataExtractReport";
            public const string DataQualityReport = "TaskGenerateDataQualityReport";
            public const string PeriodEndMetricsReport = "TaskGeneratePeriodEndMetricsReport";
            public const string ProviderSubmissionsReport = "TaskGenerateProviderSubmissionsReport";
            public const string ActCountReport = "TaskGenerateActCountReport";

            public static IEnumerable<string> TasksList = new List<string>()
            {
                CollectionStatsReport,
                DataExtractReport,
                DataQualityReport,
                PeriodEndMetricsReport,
                ProviderSubmissionsReport,
                ActCountReport
            };
        }
    }
}
