using System.Collections.Generic;
using System.Threading;
using ESFA.DC.PeriodEnd.ReportService.Reports.Interface;
using ESFA.DC.PeriodEnd.ReportService.Reports.Interface.AppsCoInvestment.Model;
using ESFA.DC.PeriodEnd.ReportService.Reports.Interface.AppsCoInvestment.Persistence;
using ESFA.DC.PeriodEnd.ReportService.Reports.Interface.AppsCoInvestment.Persistence.Model;

namespace ESFA.DC.PeriodEnd.ReportService.Reports.AppsCoInvestment
{
    public class AppsCoInvestmentPersistenceMapper : IAppsCoInvestmentPersistenceMapper
    {
        public IEnumerable<AppsCoInvestmentPersistModel> MapAsync(IReportServiceContext reportServiceContext, IEnumerable<AppsCoInvestmentRecord> appsCoInvestmentRecords,
            CancellationToken cancellationToken)
        {
            var persistModels = new List<AppsCoInvestmentPersistModel>();

            foreach (var record in appsCoInvestmentRecords)
            {
                var persistModel = new AppsCoInvestmentPersistModel()
                {
                    Ukprn = reportServiceContext.Ukprn,
                    LearnRefNumber = record.RecordKey.LearnerReferenceNumber,
                    FamilyName = record.FamilyName,
                    GivenNames = record.GivenNames,
                    UniqueLearnerNumber = record.UniqueLearnerNumber,
                    LearningStartDate = record.RecordKey.LearningStartDate,
                    ProgType = record.RecordKey.ProgrammeType,
                    StandardCode = record.RecordKey.StandardCode,
                    FrameworkCode = record.RecordKey.FrameworkCode,
                    ApprenticeshipPathway = record.RecordKey.PathwayCode,
                    SoftwareSupplierAimIdentifier = record.LearningDelivery.SWSupAimId,
                    LearningDeliveryFAMTypeApprenticeshipContractType = record.ApprenticeshipContractType,
                    EmployerIdentifierAtStartOfLearning = record.EmployerIdentifierAtStartOfLearning,
                    EmployerNameFromApprenticeshipService = record.EmployerNameFromApprenticeshipService,
                    TotalPMRPreviousFundingYears = record.EarningsAndPayments.TotalPMRPreviousFundingYears,
                    TotalCoInvestmentDueFromEmployerInPreviousFundingYears = record.EarningsAndPayments.TotalCoInvestmentDueFromEmployerInPreviousFundingYears,
                    TotalPMRThisFundingYear = record.EarningsAndPayments.TotalPMRThisFundingYear,
                    TotalCoInvestmentDueFromEmployerThisFundingYear = record.EarningsAndPayments.TotalCoInvestmentDueFromEmployerThisFundingYear,
                    PercentageOfCoInvestmentCollected = record.EarningsAndPayments.PercentageOfCoInvestmentCollected,
                    LDM356Or361 = record.LDM356Or361,
                    CoInvestmentDueFromEmployerForAugust = record.EarningsAndPayments.CoInvestmentPaymentsDueFromEmployer.August,
                    CoInvestmentDueFromEmployerForSeptember = record.EarningsAndPayments.CoInvestmentPaymentsDueFromEmployer.September,
                    CoInvestmentDueFromEmployerForOctober = record.EarningsAndPayments.CoInvestmentPaymentsDueFromEmployer.October,
                    CoInvestmentDueFromEmployerForNovember = record.EarningsAndPayments.CoInvestmentPaymentsDueFromEmployer.November,
                    CoInvestmentDueFromEmployerForDecember = record.EarningsAndPayments.CoInvestmentPaymentsDueFromEmployer.December,
                    CoInvestmentDueFromEmployerForJanuary = record.EarningsAndPayments.CoInvestmentPaymentsDueFromEmployer.January,
                    CoInvestmentDueFromEmployerForFebruary = record.EarningsAndPayments.CoInvestmentPaymentsDueFromEmployer.February,
                    CoInvestmentDueFromEmployerForMarch = record.EarningsAndPayments.CoInvestmentPaymentsDueFromEmployer.March,
                    CoInvestmentDueFromEmployerForApril = record.EarningsAndPayments.CoInvestmentPaymentsDueFromEmployer.April,
                    CoInvestmentDueFromEmployerForMay = record.EarningsAndPayments.CoInvestmentPaymentsDueFromEmployer.May,
                    CoInvestmentDueFromEmployerForJune = record.EarningsAndPayments.CoInvestmentPaymentsDueFromEmployer.June,
                    CoInvestmentDueFromEmployerForJuly = record.EarningsAndPayments.CoInvestmentPaymentsDueFromEmployer.July,
                    CoInvestmentDueFromEmployerForR13 = record.EarningsAndPayments.CoInvestmentPaymentsDueFromEmployer.R13,
                    CoInvestmentDueFromEmployerForR14 = record.EarningsAndPayments.CoInvestmentPaymentsDueFromEmployer.R14,
                    CompletionEarningThisFundingYear = record.EarningsAndPayments.CompletionEarningThisFundingYear,
                    CompletionPaymentsThisFundingYear = record.EarningsAndPayments.CompletionPaymentsThisFundingYear,
                    EmployerCoInvestmentPercentage = record.EarningsAndPayments.EmployerCoInvestmentPercentage,
                    ApplicableProgrammeStartDate = record.LearningDelivery.AECLearningDelivery.AppAdjLearnStartDate
                };
                persistModels.Add(persistModel);
            }
            return persistModels;
        }
    }
}
