using System;

namespace ESFA.DC.PeriodEnd.ReportService.Reports.Interface.DataQuality.Model
{
    public class ProviderCount
    {
        public long Ukprn { get; set; }

        public int NoOfInvalidLearners { get; set; }

        public int NoOfValidLearners { get; set; }

        public string LatestFileName { get; set; }

        public DateTime SubmittedDateTime { get; set; }

        public string LatestReturn { get; set; }

        public string Name { get; set; }

        public string Status { get; set; }
    }
}
