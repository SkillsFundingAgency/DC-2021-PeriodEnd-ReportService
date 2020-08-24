using System;

namespace ESFA.DC.PeriodEnd.ReportService.Reports.Interface.ProviderSubmissions.Model
{
    public class FileDetails
    {
        public string Filename { get; set; }

        public long Ukprn { get; set; }

        public DateTime? SubmittedTime { get; set; }

        public int? TotalErrorCount { get; set; }

        public int? TotalInvalidLearnersSubmitted { get; set; }

        public int? TotalValidLearnersSubmitted { get; set; }

        public int? TotalWarningCount { get; set; }
    }
}
