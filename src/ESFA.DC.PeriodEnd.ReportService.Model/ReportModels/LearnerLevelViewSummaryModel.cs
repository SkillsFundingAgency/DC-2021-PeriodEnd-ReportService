using System;
using System.Collections.Generic;
using System.Text;

namespace ESFA.DC.PeriodEnd.ReportService.Model.ReportModels
{
    public class LearnerLevelViewSummaryModel
    {
        public int? Ukprn { get; set; }

        public int NumberofLearners { get; set; }

        public int NumberofDatalocks { get; set; }

        public int NumberofClawbacks { get; set; }

        public int NumberofHBCP { get; set; }

        public int NumberofOthers { get; set; }

        public int NumberofCoInvestmentsToCollect { get; set; }

        public int NumberofEarningsReleased { get; set; }

        public decimal? EarningsReleased { get; set; }

        public decimal? TotalEarningsForThisPeriod { get; set; }

        public decimal? TotalCostOfDataLocksForThisPeriod { get; set; }

        public decimal? TotalCostOfHBCPForThisPeriod { get; set; }

        public decimal? TotalCostofClawbackForThisPeriod { get; set; }

        public decimal? TotalCostofOthersForThisPeriod { get; set; }

        public decimal? ESFAPlannedPaymentsForThisPeriod { get; set; }

        public decimal? CoInvestmentPaymentsToCollectForThisPeriod { get; set; }

        public decimal? TotalPaymentsForThisPeriod { get; set; }

        public decimal? TotalEarningsToDate { get; set; }

        public decimal? TotalPaymentsToDate { get; set; }

        public decimal? TotalCoInvestmentCollectedToDate { get; set; }
    }
}
