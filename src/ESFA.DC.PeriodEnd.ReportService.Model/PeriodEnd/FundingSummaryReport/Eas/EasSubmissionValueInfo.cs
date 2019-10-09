using System;

namespace ESFA.DC.PeriodEnd.ReportService.Model.PeriodEnd.FundingSummaryReport.Eas
{
    public class EasSubmissionValueInfo
    {
        public Guid SubmissionId { get; set; }

        public byte? CollectionPeriod { get; set; }

        public int? PaymentId { get; set; }

        public decimal? PaymentValue { get; set; }
    }
}
