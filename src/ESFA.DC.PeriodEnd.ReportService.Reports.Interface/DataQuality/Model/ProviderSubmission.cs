using System;

namespace ESFA.DC.PeriodEnd.ReportService.Reports.Interface.DataQuality.Model
{
    public class ProviderSubmission
    {
        public long Ukprn { get; set; }

        public DateTime LatestFileSubmitted { get; set; }

        public string OrgName { get; set; }
    }
}
