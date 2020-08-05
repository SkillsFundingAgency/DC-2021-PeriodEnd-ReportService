using System;

namespace ESFA.DC.PeriodEnd.ReportService.Reports.Interface.DataQuality.Model
{
    public class FilePeriodInfo
    {
        public long UKPRN { get; set; }

        public string Filename { get; set; }

        public int PeriodNumber { get; set; }

        public DateTime? SubmittedTime { get; set; }
    }
}
