using System;

namespace ESFA.DC.PeriodEnd.ReportService.Model.InternalReports.DataQualityReport
{
    public sealed class Top10ProvidersWithInvalidLearners
    {
        public long Ukprn { get; set; }

        public string Name { get; set; }

        public string Status { get; set; }

        public string LatestReturn { get; set; }

        public int NoOfValidLearners { get; set; }

        public int NoOfInvalidLearners { get; set; }

        public string LatestFileName { get; set; }

        public DateTime SubmittedDateTime { get; set; }
    }
}
