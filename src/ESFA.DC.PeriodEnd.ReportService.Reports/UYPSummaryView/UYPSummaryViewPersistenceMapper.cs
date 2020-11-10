using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using ESFA.DC.PeriodEnd.ReportService.Reports.Interface;
using ESFA.DC.PeriodEnd.ReportService.Reports.Interface.UYPSummaryView.Model;
using ESFA.DC.PeriodEnd.ReportService.Reports.Interface.UYPSummaryView.Persistence;
using ESFA.DC.ReportData.Model;

namespace ESFA.DC.PeriodEnd.ReportService.Reports.AppsMonthly
{
    public class UYPSummaryViewPersistenceMapper : IUYPSummaryViewPersistenceMapper
    {
        public IEnumerable<LearnerLevelViewReport> Map(IReportServiceContext reportServiceContext, IEnumerable<LearnerLevelViewModel> learnerLevelViewRecords, CancellationToken cancellationToken)
        {
            return learnerLevelViewRecords.Select(m => new LearnerLevelViewReport
            {
                Ukprn = reportServiceContext.Ukprn,
                PaymentLearnerReferenceNumber = m.PaymentLearnerReferenceNumber,
                PaymentUniqueLearnerNumber = long.Parse(m.PaymentUniqueLearnerNumbers.Split(';')[0]),
                FamilyName = m.FamilyName,
                GivenNames = m.GivenNames,
                LearnerEmploymentStatusEmployerId = m.LearnerEmploymentStatusEmployerId,
                EmployerName = m.EmployerName,
                TotalEarningsToDate = m.TotalEarningsToDate,
                PlannedPaymentsToYouToDate = m.PlannedPaymentsToYouToDate,
                TotalCoInvestmentCollectedToDate = m.TotalCoInvestmentCollectedToDate,
                CoInvestmentOutstandingFromEmplToDate = m.CoInvestmentOutstandingFromEmplToDate,
                TotalEarningsForPeriod = m.TotalEarningsForPeriod,
                ESFAPlannedPaymentsThisPeriod = m.ESFAPlannedPaymentsThisPeriod,
                CoInvestmentPaymentsToCollectThisPeriod = m.CoInvestmentPaymentsToCollectThisPeriod,
                IssuesAmount = m.IssuesAmount,
                ReasonForIssues = m.ReasonForIssues,
                PaymentFundingLineType = m.PaymentFundingLineType,
                RuleDescription = m.RuleDescription,
                ReturnPeriod = reportServiceContext.ReturnPeriod
            });
        }
    }
}
