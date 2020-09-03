using System;

namespace ESFA.DC.PeriodEnd.ReportService.Reports.Interface.UYPSummaryView.Model
{
    public class Payment
    {
        public int AcademicYear { get; set; }

        public string LearnerReferenceNumber { get; set; }

        public long LearnerUln { get; set; }

        public string LearningAimReference { get; set; }

        public string ReportingAimFundingLineType { get; set; }

        public byte CollectionPeriod { get; set; }

        public decimal Amount { get; set; }

        public byte TransactionType { get; set; }

        public byte FundingSource { get; set; }

        public long ApprenticeshipId { get; set; }
    }
}
