using System;
using System.Collections.Generic;
using System.Text;

namespace ESFA.DC.PeriodEnd.ReportService.Reports.Interface.UYPSummaryView.Model
{
    public class LearnerLevelViewModel
    {
        public int? Ukprn { get; set; }

        public string PaymentLearnerReferenceNumber { get; set; }

        public long? PaymentUniqueLearnerNumber { get; set; }

        public string FamilyName { get; set; }

        public string GivenNames { get; set; }

        public int? LearnerEmploymentStatusEmployerId { get; set; }

        public string EmployerName { get; set; }

        public decimal? TotalEarningsToDate { get; set; }

        public decimal? PlannedPaymentsToYouToDate { get; set; }

        public decimal? TotalCoInvestmentCollectedToDate { get; set; }

        public decimal? CoInvestmentOutstandingFromEmplToDate { get; set; }

        public decimal? TotalEarningsForPeriod { get; set; }

        public decimal? ESFAPlannedPaymentsThisPeriod { get; set; }

        public decimal? CoInvestmentPaymentsToCollectThisPeriod { get; set; }

        public decimal? IssuesAmount { get; set; }

        public string ReasonForIssues { get; set; }

        public string PaymentFundingLineType { get; set; }

        public string RuleDescription { get; set; }
    }
}
