using System;

namespace ESFA.DC.PeriodEnd.ReportService.Reports.Interface.DataQuality.Model
{
    public class DataQualityModel
    {
        public string Description { get; set; }

        public string Collection { get; set; }

        public int NoOfProviders { get; set; }

        public int NoOfValidFilesSubmitted { get; set; }

        public DateTime? EarliestValidSubmission { get; set; }

        public DateTime? LastValidSubmission { get; set; }
    }
}
