using System;
using System.Collections.Generic;
using System.Linq;
using ESFA.DC.PeriodEnd.ReportService.Interface.Builders;
using ESFA.DC.PeriodEnd.ReportService.Model.PeriodEnd.AppsCoInvestment;
using ESFA.DC.PeriodEnd.ReportService.Service.Constants;
using ESFA.DC.PeriodEnd.ReportService.Service.Extensions;

namespace ESFA.DC.PeriodEnd.ReportService.Service.Builders
{
    public class AppsCoInvestmentContributionsModelBuilder : IAppsCoInvestmentContributionsModelBuilder
    {
        private readonly int _fundingSource = 3;
        private readonly int[] _transactionTypes =
        {
            Constants.DASPayments.TransactionType.Learning_On_Programme,
            Constants.DASPayments.TransactionType.Completion,
            Constants.DASPayments.TransactionType.Balancing,
        };

        public bool IsValidLearner(LearnerInfo learner)
        {
            if (learner.LearningDeliveries.Any(x => x.AppFinRecords.Any(y => y.AFinType.CaseInsensitiveEquals(Generics.PMR))))
            {
                return true;
            }

            return false;
        }

        public IEnumerable<AppsCoInvestmentContributionsModel> BuildModel(
            AppsCoInvestmentILRInfo appsCoInvestmentIlrInfo,
            AppsCoInvestmentRulebaseInfo appsCoInvestmentRulebaseInfo,
            AppsCoInvestmentPaymentsInfo appsCoInvestmentPaymentsInfo)
        {
            List<AppsCoInvestmentContributionsModel> appsCoInvestmentContributionsModels = new List<AppsCoInvestmentContributionsModel>();
            foreach (var learner in appsCoInvestmentIlrInfo.Learners)
            {
                var paymentGroups = appsCoInvestmentPaymentsInfo.Payments.Where(x =>
                        x.LearnerReferenceNumber.CaseInsensitiveEquals(learner.LearnRefNumber))
                    .GroupBy(x => new
                    {
                        x.LearnerReferenceNumber,
                        x.LearningStartDate,
                        x.LearningAimProgrammeType,
                        x.LearningAimStandardCode,
                        x.LearningAimFrameworkCode,
                        x.LearningAimPathwayCode
                    }).Select(x => new
                    {
                        x.Key.LearnerReferenceNumber,
                        x.Key.LearningStartDate,
                        x.Key.LearningAimProgrammeType,
                        x.Key.LearningAimStandardCode,
                        x.Key.LearningAimFrameworkCode,
                        x.Key.LearningAimPathwayCode,
                        PaymentInfoList = x.ToList()
                    }).ToList();

                var ilrLearningDeliveriesInfo = learner.LearningDeliveries.Where(x => x.AppFinRecords.Any(y =>
                    y.AFinType.CaseInsensitiveEquals(Generics.PMR) &&
                    y.LearnRefNumber.CaseInsensitiveEquals(learner.LearnRefNumber))).ToList();

                var rulebaseInfo = appsCoInvestmentRulebaseInfo.AECApprenticeshipPriceEpisodePeriodisedValues.Where(x =>
                    x.AttributeName.CaseInsensitiveEquals("PriceEpisodeCompletionPayment") &&
                    x.Periods.All(p => p != decimal.Zero) &&
                    x.LearnRefNumber.CaseInsensitiveEquals(learner.LearnRefNumber)).ToList();

                if (ilrLearningDeliveriesInfo.Any() || rulebaseInfo.Any() || paymentGroups.Any(x => x.PaymentInfoList.Any(y => y.FundingSource == _fundingSource) ||
                    x.PaymentInfoList.Any(y => y.TransactionType == 3)))
                {
                    foreach (var payment in paymentGroups)
                    {
                        var paymentInfo = payment.PaymentInfoList.First();
                        var learningDelivery = ilrLearningDeliveriesInfo.FirstOrDefault(x => x.UKPRN == paymentInfo.UkPrn &&
                                                                           x.LearnRefNumber.CaseInsensitiveEquals(payment.LearnerReferenceNumber) &&
                                                                           x.LearnAimRef.CaseInsensitiveEquals(paymentInfo.LearningAimReference) &&
                                                                           x.LearnStartDate == payment.LearningStartDate &&
                                                                           x.ProgType == payment.LearningAimProgrammeType &&
                                                                           x.StdCode == payment.LearningAimProgrammeType &&
                                                                           x.FworkCode == payment.LearningAimFrameworkCode &&
                                                                           x.PwayCode == payment.LearningAimPathwayCode);

                        if (learningDelivery == null)
                        {
                            continue;
                        }

                        var prevYearAppFinData = learningDelivery.AppFinRecords?
                            .Where(x => x.AFinDate < Generics.BeginningOfYear &&
                                        x.AFinType.CaseInsensitiveEquals(Generics.PMR)).ToList();

                        var currentYearAppFinData = learningDelivery.AppFinRecords?
                            .Where(x => x.AFinDate >= Generics.BeginningOfYear &&
                                        x.AFinDate <= Generics.EndOfYear &&
                                        x.AFinType.CaseInsensitiveEquals(Generics.PMR)).ToList();

                        var aecLearningDeliveryInfo =
                            appsCoInvestmentRulebaseInfo.AECLearningDeliveries.FirstOrDefault(x =>
                                x.LearnRefNumber.CaseInsensitiveEquals(payment.LearnerReferenceNumber) &&
                                x.AimSeqNumber == learningDelivery.AimSeqNumber);

                        bool learnDelMathEng =
                            aecLearningDeliveryInfo?.LearningDeliveryValues.LearnDelMathEng ?? false;

                        bool flagCalculateCoInvestmentAmount =
                            !learnDelMathEng && (paymentInfo?.FundingSource == _fundingSource &&
                                                 _transactionTypes.Any(x => x == paymentInfo?.TransactionType)) &&
                                                 paymentInfo.AcademicYear == Generics.AcademicYear;

                        var model = new AppsCoInvestmentContributionsModel
                        {
                            LearnRefNumber = payment.LearnerReferenceNumber,
                            UniqueLearnerNumber = !payment.PaymentInfoList.Select(x => x.LearnerUln).Distinct().Any() ?
                                payment.PaymentInfoList.First().LearnerUln : (long?)null,
                            LearningStartDate = payment.LearningStartDate?.ToString("dd/MM/yyyy"),
                            ProgType = payment.LearningAimProgrammeType,
                            StandardCode = payment.LearningAimStandardCode,
                            FrameworkCode = payment.LearningAimFrameworkCode,
                            ApprenticeshipPathway = payment.LearningAimPathwayCode,
                            SoftwareSupplierAimIdentifier = ilrLearningDeliveriesInfo.Where(x => x.LearnAimRef.CaseInsensitiveEquals(Generics.ZPROG001)).Select(x => x.SWSupAimId).FirstOrDefault(),
                            LearningDeliveryFAMTypeApprenticeshipContractType = !payment.PaymentInfoList.Select(x => x.ContractType).Distinct().Any() ?
                                payment.PaymentInfoList.First().ContractType : (byte?)null,
                            EmployerIdentifierAtStartOfLearning = learner.LearnerEmploymentStatus
                                .Where(x => x.DateEmpStatApp <= payment.LearningStartDate)
                                .OrderByDescending(x => x.DateEmpStatApp).FirstOrDefault()?.EmpId,
                            ApplicableProgrammeStartDate = aecLearningDeliveryInfo.AppAdjLearnStartDate,
                            TotalPMRPreviousFundingYears =
                                prevYearAppFinData.Where(x => x.AFinCode == 1 || x.AFinCode == 2)
                                    .Sum(x => x.AFinAmount) -
                                prevYearAppFinData.Where(x => x.AFinCode == 3).Sum(x => x.AFinAmount),
                            TotalCoInvestmentDueFromEmployerInPreviousFundingYears = payment.PaymentInfoList.Where(x => x.FundingSource == _fundingSource &&
                                                                                                                        _transactionTypes.Any(y => y == x.TransactionType) &&
                                                                                                                        x.AcademicYear != Generics.AcademicYear).Select(x => x.Amount).Sum(),
                            TotalPMRThisFundingYear = currentYearAppFinData.Where(x => x.AFinCode == 1 || x.AFinCode == 2)
                                                          .Sum(x => x.AFinAmount) -
                                                      currentYearAppFinData.Where(x => x.AFinCode == 3).Sum(x => x.AFinAmount),
                            LDM356Or361 = learningDelivery.LearningDeliveryFAMs.Any(x =>
                                (x.LearnDelFAMType.CaseInsensitiveEquals(Generics.LearningDeliveryFAMCodeLDM) && x.LearnDelFAMCode.CaseInsensitiveEquals(Generics.LearningDeliveryFAMCode356)) ||
                                (x.LearnDelFAMType.CaseInsensitiveEquals(Generics.LearningDeliveryFAMCodeLDM) && x.LearnDelFAMCode.CaseInsensitiveEquals(Generics.LearningDeliveryFAMCode361)))
                                ? "Yes"
                                : "No",
                            CompletionEarningThisFundingYear = rulebaseInfo.SelectMany(x => x.Periods).Sum(),
                            CompletionPaymentsThisFundingYear = payment.PaymentInfoList.Where(x => x.TransactionType == 3 && x.AcademicYear == Generics.AcademicYear).Select(x => x.Amount).Sum(),
                            CoInvestmentDueFromEmployerForAugust = CalculateCoInvestmentDueForMonth(flagCalculateCoInvestmentAmount, payment.PaymentInfoList, 1),
                            CoInvestmentDueFromEmployerForSeptember = CalculateCoInvestmentDueForMonth(flagCalculateCoInvestmentAmount, payment.PaymentInfoList, 2),
                            CoInvestmentDueFromEmployerForOctober = CalculateCoInvestmentDueForMonth(flagCalculateCoInvestmentAmount, payment.PaymentInfoList, 3),
                            CoInvestmentDueFromEmployerForNovember = CalculateCoInvestmentDueForMonth(flagCalculateCoInvestmentAmount, payment.PaymentInfoList, 4),
                            CoInvestmentDueFromEmployerForDecember = CalculateCoInvestmentDueForMonth(flagCalculateCoInvestmentAmount, payment.PaymentInfoList, 5),
                            CoInvestmentDueFromEmployerForJanuary = CalculateCoInvestmentDueForMonth(flagCalculateCoInvestmentAmount, payment.PaymentInfoList, 6),
                            CoInvestmentDueFromEmployerForFebruary = CalculateCoInvestmentDueForMonth(flagCalculateCoInvestmentAmount, payment.PaymentInfoList, 7),
                            CoInvestmentDueFromEmployerForMarch = CalculateCoInvestmentDueForMonth(flagCalculateCoInvestmentAmount, payment.PaymentInfoList, 8),
                            CoInvestmentDueFromEmployerForApril = CalculateCoInvestmentDueForMonth(flagCalculateCoInvestmentAmount, payment.PaymentInfoList, 9),
                            CoInvestmentDueFromEmployerForMay = CalculateCoInvestmentDueForMonth(flagCalculateCoInvestmentAmount, payment.PaymentInfoList, 10),
                            CoInvestmentDueFromEmployerForJune = CalculateCoInvestmentDueForMonth(flagCalculateCoInvestmentAmount, payment.PaymentInfoList, 11),
                            CoInvestmentDueFromEmployerForJuly = CalculateCoInvestmentDueForMonth(flagCalculateCoInvestmentAmount, payment.PaymentInfoList, 12),
                            CoInvestmentDueFromEmployerForR13 = CalculateCoInvestmentDueForMonth(flagCalculateCoInvestmentAmount, payment.PaymentInfoList, 13),
                            CoInvestmentDueFromEmployerForR14 = CalculateCoInvestmentDueForMonth(flagCalculateCoInvestmentAmount, payment.PaymentInfoList, 14),
                        };

                        model.TotalCoInvestmentDueFromEmployerThisFundingYear =
                            model.CoInvestmentDueFromEmployerForAugust + model.CoInvestmentDueFromEmployerForSeptember + model.CoInvestmentDueFromEmployerForOctober +
                            model.CoInvestmentDueFromEmployerForNovember + model.CoInvestmentDueFromEmployerForDecember + model.CoInvestmentDueFromEmployerForJanuary +
                            model.CoInvestmentDueFromEmployerForFebruary + model.CoInvestmentDueFromEmployerForMarch + model.CoInvestmentDueFromEmployerForApril +
                            model.CoInvestmentDueFromEmployerForMay + model.CoInvestmentDueFromEmployerForJune + model.CoInvestmentDueFromEmployerForJuly +
                            model.CoInvestmentDueFromEmployerForR13 + model.CoInvestmentDueFromEmployerForR14;

                        model.PercentageOfCoInvestmentCollected =
                            model.TotalCoInvestmentDueFromEmployerInPreviousFundingYears +
                            model.TotalCoInvestmentDueFromEmployerThisFundingYear == 0
                                ? 0
                                : ((model.TotalPMRPreviousFundingYears + model.TotalPMRThisFundingYear) /
                                   (model.TotalCoInvestmentDueFromEmployerInPreviousFundingYears +
                                    model.TotalCoInvestmentDueFromEmployerThisFundingYear)) * 100;

                        var minSfaContributionPercentage = payment.PaymentInfoList.Where(x =>
                                x.FundingSource == _fundingSource && _transactionTypes.Any(y => y == x.TransactionType))
                            .GroupBy(x => x.DeliveryPeriod).Select(x => new
                            {
                                SfaContributionPercentage = x.ToList().Select(y => y.SfaContributionPercentage)
                            }).ToList().Min(x => x.SfaContributionPercentage).First();

                        model.EmployerCoInvestmentPercentage = (1 - minSfaContributionPercentage) * 100;

                        model.EmployerNameFromApprenticeshipService = payment.PaymentInfoList
                            .OrderBy(x => x.DeliveryPeriod).Select(x => x.EmployerName).FirstOrDefault();

                        appsCoInvestmentContributionsModels.Add(model);
                    }
                }
            }

            return appsCoInvestmentContributionsModels;
        }

        private decimal CalculateCoInvestmentDueForMonth(bool flag, List<PaymentInfo> paymentInfoList, int deliveryPeriod)
        {
            return flag
                ? paymentInfoList.Where(x => x.DeliveryPeriod == deliveryPeriod).Select(x => x.Amount).Sum()
                : 0;
        }
    }
}