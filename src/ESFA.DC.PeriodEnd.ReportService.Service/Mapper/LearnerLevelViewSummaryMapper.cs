using System;
using System.Globalization;
using CsvHelper.Configuration;
using ESFA.DC.PeriodEnd.ReportService.Model.ReportModels;

namespace ESFA.DC.PeriodEnd.ReportService.Service.Mapper
{
    public class LearnerLevelViewSummaryMapper : ClassMap<LearnerLevelViewSummaryModel>
    {
        public LearnerLevelViewSummaryMapper()
        {
            int i = 0;

            Map(m => m.Ukprn).Index(i++).Name("UKPRN");
            Map(m => m.NumberofLearners).Index(i++).Name("Number of Learners");
            Map(m => m.NumberofDatalocks).Index(i++).Name("Number of Learners with Datalocks");
            Map(m => m.NumberofClawbacks).Index(i++).Name("Number of Learners with Clawbacks");
            Map(m => m.NumberofHBCP).Index(i++).Name("Number of Learners with Hold Back Completion Payments");
            Map(m => m.NumberofOthers).Index(i++).Name("Number of Learners with Other Issues");
            Map(m => m.NumberofCoInvestmentsToCollect).Index(i++).Name("Number of Co Investments to collect");
            Map(m => m.NumberofEarningsReleased).Index(i++).Name("Number of Earnings Released");
            Map(m => m.EarningsReleased).Index(i++).Name("Earnings Released");
            Map(m => m.TotalEarningsForThisPeriod).Index(i++).Name("Total earnings for this period");
            Map(m => m.TotalCostOfDataLocksForThisPeriod).Index(i++).Name("Total cost of datalocks for this period");
            Map(m => m.TotalCostOfHBCPForThisPeriod).Index(i++).Name("Total cost of hold back completion payments for this period");
            Map(m => m.TotalCostofClawbackForThisPeriod).Index(i++).Name("Total cost of clawbacks for this period");
            Map(m => m.TotalCostofOthersForThisPeriod).Index(i++).Name("Total cost of other non-payments for this period");
            Map(m => m.ESFAPlannedPaymentsForThisPeriod).Index(i++).Name("Total ESFA planned payments for this period");
            Map(m => m.CoInvestmentPaymentsToCollectForThisPeriod).Index(i++).Name("Total Co-Investment to collect for this period");
            Map(m => m.TotalPaymentsForThisPeriod).Index(i++).Name("Total payments for this period");
            Map(m => m.TotalPaymentsToDate).Index(i++).Name("Total payments to date");
            Map(m => m.TotalEarningsToDate).Index(i++).Name("Total earnings to date");
            Map(m => m.TotalCoInvestmentCollectedToDate).Index(i++).Name("Total co-investment collected to date");
        }
    }
}
