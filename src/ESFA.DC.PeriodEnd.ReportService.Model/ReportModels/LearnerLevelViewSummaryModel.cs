using System;
using System.Collections.Generic;
using System.Text;

namespace ESFA.DC.PeriodEnd.ReportService.Model.ReportModels
{
    public class LearnerLevelViewSummaryModel
    {
        public int? Ukprn { get; set; }

        public int NumberofLearners { get; set; }

        public decimal? TotalEarningsForThisPeriod { get; set; }

        public decimal? TotalCostOfDataLocks { get; set; }

        public decimal? TotalCostOfHBCP { get; set; }

        public decimal? TotalCostofClawback { get; set; }

        public decimal? TotalCostofOthers { get; set; }

        public decimal? ESFAPlannedPayments { get; set; }

        public decimal? CoInvestmentPaymentsToCollect { get; set; }

        public decimal? TotalPayments { get; set; }
    }
}
