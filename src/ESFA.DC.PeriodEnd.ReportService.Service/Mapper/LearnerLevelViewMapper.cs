using System;
using System.Globalization;
using CsvHelper.Configuration;
using ESFA.DC.PeriodEnd.ReportService.Model.ReportModels;

namespace ESFA.DC.PeriodEnd.ReportService.Service.Mapper
{
    public class LearnerLevelViewMapper : ClassMap<LearnerLevelViewModel>
    {
        public LearnerLevelViewMapper()
        {
            int i = 0;

            Map(m => m.PaymentLearnerReferenceNumber).Index(i++).Name("Learner reference number");
            Map(m => m.PaymentUniqueLearnerNumber).Index(i++).Name("Unique learner number");

            Map(m => m.PaymentFundingLineType).Index(i++).Name("Funding line type");
            Map(m => m.LearnerEmploymentStatusEmployerId).Index(i++).Name("Employer identifier on employment status date");

            Map(m => m.Ukprn).Index(i++).Name("UKPRN");
            Map(m => m.FamilyName).Index(i++).Name("Family Name");
            Map(m => m.GivenNames).Index(i++).Name("Given Names");

            Map(m => m.TotalEarningsToDate).Index(i++).Name("Total earnings to date");
            Map(m => m.PlannedPaymentsToYouToDate).Index(i++).Name("Planned payments to you to date");
            Map(m => m.TotalCoInvestmentCollectedToDate).Index(i++).Name("Total Co-Investment collected to date");
            Map(m => m.CoInvestmentOutstandingFromEmplToDate).Index(i++).Name("Co-Investment outstanding from employer to date");
            Map(m => m.TotalEarningsForPeriod).Index(i++).Name("Total earnings from period");
            Map(m => m.ESFAPlannedPaymentsThisPeriod).Index(i++).Name("ESFA planned payments for this period");
            Map(m => m.CoInvestmentPaymentsToCollectThisPeriod).Index(i++).Name("Co-Investments to collect for this period");
            Map(m => m.IssuesAmount).Index(i++).Name("Issues amount for this period");
            Map(m => m.ReasonForIssues).Index(i++).Name("Reasons for issues");
        }
    }
}
