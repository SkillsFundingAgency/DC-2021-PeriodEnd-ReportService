using System;
using System.Collections.Generic;
using System.Text;

namespace ESFA.DC.PeriodEnd.ReportService.Model.ReportModels
{
    public class LearnerLevelViewModel
    {
        public int? Ukprn { get; set; }

        public string PaymentLearnerReferenceNumber { get; set; }

        public long? PaymentUniqueLearnerNumber { get; set; }

        public string LearningAimReference { get; set; }

        public DateTime? LearningStartDate { get; set; }

        public int? LearningAimProgrammeType { get; set; }

        public int? LearningAimStandardCode { get; set; }

        public int? LearningAimFrameworkCode { get; set; }

        public int? LearningAimPathwayCode { get; set; }

        public string FamilyName { get; set; }

        public string GivenNames { get; set; }

        public int? LearnerEmploymentStatusEmployerId { get; set; }

        public decimal? TotalEarningsToDate { get; set; }

        public decimal? PlannedPaymentsToYouToDate { get; set; }

        public decimal? TotalCoInvestmentCollectedToDate { get; set; }

        public decimal? CoInvestmentOutstandingFromEmplToDate { get; set; }

        public decimal? TotalEarningsForPeriod { get; set; }

        public decimal? ESFAPlannedPaymentsThisPeriod { get; set; }

        public decimal? CoInvestmentPaymentsToCollectThisPeriod { get; set; }

        public decimal? IssuesAmount { get; set; }

        public int? ReasonForIssues { get; set; }

        public string PaymentFundingLineType { get; set; }

        public byte?[] learningAimSeqNumbers { get; set; }
    }
}
