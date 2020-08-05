using System;

namespace ESFA.DC.PeriodEnd.ReportService.Reports.Interface.DataQuality.Model
{
    public class ProviderCount
    {
        public long Ukprn { get; set; }

        public int InvalidCount { get; set; }

        public int ValidCount { get; set; }

        public string Filename { get; set; }

        public DateTime SubmittedTime { get; set; }

        public string LatestReturn { get; set; }

        public string OrgName { get; set; }

        public string Status { get; set; }
    }
}
