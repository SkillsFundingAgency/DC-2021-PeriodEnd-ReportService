using System;

namespace ESFA.DC.PeriodEnd.ReportService.Model.InternalReports.DataQualityReport
{
    public sealed class FileDetailModel
    {
        public int Ukprn { get; set; }

        public DateTime? SubmittedTime { get; set; }

        public string Filename { get; set; }
    }
}
