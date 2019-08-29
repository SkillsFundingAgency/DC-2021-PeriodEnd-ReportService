using System;
using System.Collections.Generic;
using System.Linq;
using ESFA.DC.ILR.Model.Interface;
using ESFA.DC.PeriodEnd.ReportService.Interface.Builders;
using ESFA.DC.PeriodEnd.ReportService.Model.PeriodEnd.AppsCoInvestment;
using ESFA.DC.PeriodEnd.ReportService.Model.PeriodEnd.AppsMonthlyPayment;
using ESFA.DC.PeriodEnd.ReportService.Model.ReportModels.PeriodEnd;
using ESFA.DC.PeriodEnd.ReportService.Service.Extensions;

namespace ESFA.DC.PeriodEnd.ReportService.Service.Builders
{
    public class AppsCoInvestmentContributionsModelBuilder : IAppsCoInvestmentContributionsModelBuilder
    {
        private const string ZPROG001 = "ZPROG001";
        private const int _fundingSource = 3;
        private readonly string[] _collectionPeriods = {
            "1819-R01",
            "1819-R02",
            "1819-R03",
            "1819-R04",
            "1819-R05",
            "1819-R06",
            "1819-R07",
            "1819-R08",
            "1819-R09",
            "1819-R10",
            "1819-R11",
            "1819-R12",
            "1819-R13",
            "1819-R14"
        };

        private readonly int[] _fundingSourceLevyPayments = { 1, 5 };
        private readonly int[] _fundingSourceCoInvestmentPayments = { 2 };
        private readonly int[] _fundingSourceCoInvestmentDueFromEmployer = { 3 };
        private readonly int[] _transactionTypesLevyPayments = { 1, 2, 3 };
        private readonly int[] _transactionTypesCoInvestmentPayments = { 1, 2, 3 };
        private readonly int[] _transactionTypesCoInvestmentFromEmployer = { 1, 2, 3 };
        private readonly int[] _transactionTypesEmployerAdditionalPayments = { 4, 6 };
        private readonly int[] _transactionTypesProviderAdditionalPayments = { 5, 7 };
        private readonly int[] _transactionTypesApprenticeshipAdditionalPayments = { 16 };
        private readonly int[] _transactionTypesEnglishAndMathsPayments = { 13, 14 };
        private readonly int[] _transactionTypesLearningSupportPayments = { 8, 9, 10, 11, 12, 15 };

        private int[] _fundingSourceEmpty => new int[] { };

        public IEnumerable<AppsCoInvestmentContributionsModel> BuildModel(
            AppsCoInvestmentILRInfo appsCoInvestmentIlrInfo,
            AppsCoInvestmentRulebaseInfo appsCoInvestmentRulebaseInfo,
            AppsCoInvestmentPaymentsInfo appsCoInvestmentPaymentsInfo)
        {
            List<AppsCoInvestmentContributionsModel> appsCoInvestmentContributionsModels = new List<AppsCoInvestmentContributionsModel>();
            foreach (var learner in appsCoInvestmentIlrInfo.Learners)
            {
                //var paymentsInfo = appsCoInvestmentPaymentsInfo.Payments.Where(x => x.LearnerReferenceNumber.CaseInsensitiveEquals(learner.LearnRefNumber) &&
                //                                                                    x.FundingSource == _fundingSource).ToList();
                var paymentGroups = appsCoInvestmentPaymentsInfo.Payments.Where(x => x.LearnerReferenceNumber.CaseInsensitiveEquals(learner.LearnRefNumber))
                    .GroupBy(x => new
                    {
                        x.UkPrn,
                        x.LearnerReferenceNumber,
                        x.LearnerUln,
                        x.LearningAimReference,
                        x.LearningStartDate,
                        x.LearningAimProgrammeType,
                        x.LearningAimStandardCode,
                        x.LearningAimFrameworkCode,
                        x.LearningAimPathwayCode,
                        x.ContractType,
                        x.LegalEntityName
                    });

                var ilrInfo = learner.LearningDeliveries.Where(x => x.AppFinRecords.Any(y =>
                    y.AFinType.CaseInsensitiveEquals("PMR") &&
                    y.LearnRefNumber.CaseInsensitiveEquals(x.LearnRefNumber))).ToList();

                if (paymentGroups.Any() || ilrInfo.Any())
                {
                    foreach (var paymentGroup in paymentGroups)
                    {
                        var learningDeliveryInfo = learner.LearningDeliveries
                            .SingleOrDefault(x => x.UKPRN == paymentGroup.Key.UkPrn &&
                                                  x.LearnRefNumber ==
                                                  paymentGroup.Key.LearnerReferenceNumber &&
                                                  x.LearnAimRef ==
                                                  paymentGroup.Key.LearningAimReference &&
                                                  x.LearnStartDate ==
                                                  paymentGroup.Key.LearningStartDate &&
                                                  x.ProgType ==
                                                  paymentGroup.Key.LearningAimProgrammeType &&
                                                  x.StdCode ==
                                                  paymentGroup.Key.LearningAimStandardCode &&
                                                  x.FworkCode ==
                                                  paymentGroup.Key.LearningAimFrameworkCode &&
                                                  x.PwayCode ==
                                                  paymentGroup.Key.LearningAimPathwayCode);

                        var prevYearAppFinData = learningDeliveryInfo?.AppFinRecords
                            .Where(x => x.AFinDate < Constants.Constants.BeginningOfYear &&
                                        string.Equals(x.AFinType, "PMR", StringComparison.OrdinalIgnoreCase)).ToList();

                        var currentYearAppFinData = learningDeliveryInfo?.AppFinRecords
                            .Where(x => x.AFinDate >= Constants.Constants.BeginningOfYear && x.AFinDate <= Constants.Constants.EndOfYear &&
                                        string.Equals(x.AFinType, "PMR", StringComparison.OrdinalIgnoreCase)).ToList();

                        var aecApprenticeshipPriceEpisode =
                            appsCoInvestmentRulebaseInfo.AECApprenticeshipPriceEpisodePeriodisedValues.SingleOrDefault(
                                x =>
                                    x.UKPRN == paymentGroup.Key.UkPrn &&
                                    x.LearnRefNumber == paymentGroup.Key.LearnerReferenceNumber);

                        var aecLearningDelivery = appsCoInvestmentRulebaseInfo.AECLearningDeliveries.SingleOrDefault(x => x.LearnRefNumber == )

                        var model = new AppsCoInvestmentContributionsModel()
                        {
                            LearnRefNumber = paymentGroup.Key.LearnerReferenceNumber,
                            UniqueLearnerNumber = paymentGroup.Key.LearnerUln,
                            LearningStartDate = paymentGroup.Key.LearningStartDate?.ToString("dd/MM/yyyy"),
                            ProgType = paymentGroup.Key.LearningAimProgrammeType,
                            StandardCode = paymentGroup.Key.LearningAimStandardCode,
                            FrameworkCode = paymentGroup.Key.LearningAimFrameworkCode,
                            ApprenticeshipPathway = paymentGroup.Key.LearningAimPathwayCode,
                            SoftwareSupplierAimIdentifier = learningDeliveryInfo?.SWSupAimId,
                            LearningDeliveryFAMTypeApprenticeshipContractType = paymentGroup.Key.ContractType,
                            EmployerIdentifierAtStartOfLearning = learner.LearnerEmploymentStatus.Where(x => x.DateEmpStatApp <= paymentGroup.Key.LearningStartDate).OrderByDescending(x => x.DateEmpStatApp).FirstOrDefault()?.EmpId,
                            EmployerNameFromApprenticeshipService = GetEmployerName(paymentGroup.Key),
                            EmployerCoInvestmentPercentage = GetCoInvestmentPercentage(),
                            ApplicableProgrammeStartDate = aecApprenticeshipPriceEpisode,
                            TotalPMRPreviousFundingYears = prevYearAppFinData.Where(x => x.AFinCode == 1 || x.AFinCode == 2).Sum(x => x.AFinAmount) -
                                                           prevYearAppFinData.Where(x => x.AFinCode == 3).Sum(x => x.AFinAmount),
                            TotalCoInvestmentDueFromEmployerInPreviousFundingYears = 0,
                            TotalCoInvestmentDueFromEmployerThisFundingYear = 0,
                            PercentageOfCoInvestmentCollected = 0,
                            LDM356Or361 = learningDeliveryInfo.LearningDeliveryFAMs.Any(x => (x.LearnDelFAMType == "LDM" && x.LearnDelFAMCode == "356") ||
                                                                                             (x.LearnDelFAMType == "LDM" && x.LearnDelFAMCode == "361")) ? "Yes" : "No",
                            CompletionEarningThisFundingYear = 
                        };
                    }
                }
            }

            return appsCoInvestmentContributionsModels;
        }
    }
}