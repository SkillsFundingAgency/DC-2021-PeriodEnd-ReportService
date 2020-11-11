using System.Collections.Generic;
using System.Linq;
using System.Threading;
using ESFA.DC.PeriodEnd.ReportService.Reports.Interface;
using ESFA.DC.PeriodEnd.ReportService.Reports.Interface.UYPSummaryView.Model;
using ESFA.DC.PeriodEnd.ReportService.Reports.Interface.UYPSummaryView.Persistence;
using ESFA.DC.ReportData.Model;

namespace ESFA.DC.PeriodEnd.ReportService.Reports.UYPSummaryView
{
    public class UYPSummaryViewPersistenceMapper : IUYPSummaryViewPersistenceMapper
    {
        public IEnumerable<LearnerLevelViewReport> Map(IReportServiceContext reportServiceContext, IEnumerable<LearnerLevelViewModel> learnerLevelViewRecords, CancellationToken cancellationToken)
        {
            return learnerLevelViewRecords.Select(m => new LearnerLevelViewReport
            {
                Ukprn = reportServiceContext.Ukprn,
                PaymentLearnerReferenceNumber = m.PaymentLearnerReferenceNumber,
                PaymentUniqueLearnerNumber = m.PaymentUniqueLearnerNumber,
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

        public IEnumerable<UYPSummaryViewReport> Map(IReportServiceContext reportServiceContext, IEnumerable<LearnerLevelViewSummaryModel> summaryModels)
        {
            return summaryModels.Select(m => new UYPSummaryViewReport
            {
                Ukprn = reportServiceContext.Ukprn,
                ReturnPeriod = reportServiceContext.ReturnPeriod,
                CoInvestmentPaymentsToCollectForThisPeriod = m.CoInvestmentPaymentsToCollectForThisPeriod,
                ESFAPlannedPaymentsForThisPeriod = m.ESFAPlannedPaymentsForThisPeriod,
                EarningsReleased = m.EarningsReleased,
                NumberofClawbacks = m.NumberofClawbacks,
                NumberofCoInvestmentsToCollect = m.NumberofCoInvestmentsToCollect,
                NumberofDatalocks = m.NumberofDatalocks,
                NumberofEarningsReleased = m.NumberofEarningsReleased,
                NumberofHBCP = m.NumberofHBCP,
                NumberofLearners = m.NumberofLearners,
                NumberofOthers = m.NumberofOthers,
                TotalCoInvestmentCollectedToDate = m.TotalCoInvestmentCollectedToDate,
                TotalCostOfDataLocksForThisPeriod = m.TotalCostOfDataLocksForThisPeriod,
                TotalCostOfHBCPForThisPeriod = m.TotalCostOfHBCPForThisPeriod,
                TotalCostofClawbackForThisPeriod = m.TotalCostofClawbackForThisPeriod,
                TotalCostofOthersForThisPeriod = m.TotalCostofOthersForThisPeriod,
                TotalEarningsForThisPeriod = m.TotalEarningsForThisPeriod,
                TotalEarningsToDate = m.TotalEarningsToDate,
                TotalPaymentsToDate = m.TotalPaymentsToDate,
                SummaryTotal = m.CoInvestmentPaymentsToCollectForThisPeriod.GetValueOrDefault()  + m.ESFAPlannedPaymentsForThisPeriod.GetValueOrDefault()
            });
        }
    }
}
