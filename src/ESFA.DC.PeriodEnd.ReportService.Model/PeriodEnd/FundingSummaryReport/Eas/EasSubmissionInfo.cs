using System;

namespace ESFA.DC.PeriodEnd.ReportService.Model.PeriodEnd.FundingSummaryReport.Eas
{
    public class EasSubmissionInfo
    {
        public Guid SubmissionId { get; set; }

        public string Ukprn { get; set; }

        public byte? CollectionPeriod { get; set; }
    }
}
