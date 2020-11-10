using System;
using System.Linq.Expressions;
using System.Text;
using CsvHelper.Configuration;
using ESFA.DC.PeriodEnd.ReportService.Reports.Abstract;
using ESFA.DC.PeriodEnd.ReportService.Reports.Interface.UYPSummaryView.Model;
using ESFA.DC.ReportData.Model;

namespace ESFA.DC.PeriodEnd.ReportService.Reports.UYPSummaryView
{
    public class UYPSummaryViewClassMap : AbstractClassMap<LearnerLevelViewModel>
    {
        public UYPSummaryViewClassMap()
        {
            MapIndex(m => m.PaymentLearnerReferenceNumber).Name("Learner reference number");
            MapIndex(m => m.PaymentUniqueLearnerNumbers).Name("Unique learner number");

            MapIndex(m => m.LearnerEmploymentStatusEmployerId).Name("Employer identifier on employment status date");
            MapIndex(m => m.EmployerName).Name("Latest employer name");

            MapIndex(m => m.Ukprn).Name("UKPRN");
            MapIndex(m => m.FamilyName).Name("Family Name");
            MapIndex(m => m.GivenNames).Name("Given Names");

            MapIndex(m => m.TotalEarningsToDate).Name("Total earnings to date");
            MapIndex(m => m.PlannedPaymentsToYouToDate).Name("ESFA planned payments to you to date");
            MapIndex(m => m.TotalCoInvestmentCollectedToDate).Name("Total Co-Investment collected to date");
            MapIndex(m => m.CoInvestmentOutstandingFromEmplToDate).Name("Co-Investment outstanding from employer to date");
            MapIndex(m => m.TotalEarningsForPeriod).Name("Total earnings for this period");
            MapIndex(m => m.ESFAPlannedPaymentsThisPeriod).Name("ESFA planned payments for this period");
            MapIndex(m => m.CoInvestmentPaymentsToCollectThisPeriod).Name("Co-investment payments to collect for this period");
            MapIndex(m => m.IssuesAmount).Name("Issues amount");
            MapIndex(m => m.ReasonForIssues).Name("Reasons for issues");
        }
    }
}
