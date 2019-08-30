using System;

namespace ESFA.DC.PeriodEnd.ReportService.Model.InternalReports.DataQualityReport
{
    public sealed class DataQualityReturningProviders
    {
        public string Description { get; set; }

        public string Collection { get; set; }

        public int NoOfProviders { get; set; }

        public int NoOfValidFilesSubmitted { get; set; }

        public DateTime? EarliestValidSubmission { get; set; }

        public DateTime? LastValidSubmission { get; set; }
    }
}
