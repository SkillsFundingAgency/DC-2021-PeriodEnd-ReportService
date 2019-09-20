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

        private readonly string _priceEpisodeCompletionPayment = "PriceEpisodeCompletionPayment";

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

                var ilrLearningDeliveriesInfo = learner.LearningDeliveries?.Where(x => x.AppFinRecords.Any(y =>
                    y.AFinType.CaseInsensitiveEquals(Generics.PMR) &&
                    y.LearnRefNumber.CaseInsensitiveEquals(learner.LearnRefNumber))).ToList();

                var rulebaseInfo = appsCoInvestmentRulebaseInfo.AECApprenticeshipPriceEpisodePeriodisedValues?.Where(x =>
                    x.AttributeName.CaseInsensitiveEquals(_priceEpisodeCompletionPayment) &&
                    x.Periods.All(p => p != decimal.Zero) &&
                    x.LearnRefNumber.CaseInsensitiveEquals(learner.LearnRefNumber)).ToList();

                if (ilrLearningDeliveriesInfo?.Count > 0 || rulebaseInfo?.Count > 0 ||
                    paymentGroups.Any(x => x.PaymentInfoList.Any(y => y.FundingSource == _fundingSource || y.TransactionType == 3)))
                {
                    foreach (var payment in paymentGroups)
                    {
                        var paymentInfo = payment.PaymentInfoList
                            .SingleOrDefault(x => x.LearningAimReference.CaseInsensitiveEquals(Generics.ZPROG001));

                        if (paymentInfo == null)
                        {
                            continue;
                        }

                        var learningDelivery = ilrLearningDeliveriesInfo?.FirstOrDefault(x => x.UKPRN == paymentInfo.UkPrn &&
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
                            appsCoInvestmentRulebaseInfo.AECLearningDeliveries?.SingleOrDefault(x =>
                                x.LearnRefNumber.CaseInsensitiveEquals(payment.LearnerReferenceNumber) &&
                                x.AimSeqNumber == learningDelivery.AimSeqNumber);

                        bool learnDelMathEng =
                            aecLearningDeliveryInfo?.LearningDeliveryValues.LearnDelMathEng ?? false;

                        bool flagCalculateCoInvestmentAmount =
                            !learnDelMathEng && (paymentInfo.FundingSource == _fundingSource &&
                                                 _transactionTypes.Any(x => x == paymentInfo.TransactionType)) &&
                                                 paymentInfo.AcademicYear == Generics.AcademicYear;

                        var model = new AppsCoInvestmentContributionsModel
                        {
                            LearnRefNumber = payment.LearnerReferenceNumber,
                            UniqueLearnerNumber = payment.PaymentInfoList.Select(x => x.LearnerUln).Distinct().Count() == 1 ?
                                paymentInfo.LearnerUln : (long?)null,
                            LearningStartDate = payment.LearningStartDate?.ToString("dd/MM/yyyy"),
                            ProgType = payment.LearningAimProgrammeType,
                            StandardCode = payment.LearningAimStandardCode,
                            FrameworkCode = payment.LearningAimFrameworkCode,
                            ApprenticeshipPathway = payment.LearningAimPathwayCode,
                            SoftwareSupplierAimIdentifier = learningDelivery.SWSupAimId ?? null,
                            LearningDeliveryFAMTypeApprenticeshipContractType = !payment.PaymentInfoList.Select(x => x.ContractType).Distinct().Any() ?
                                paymentInfo.ContractType : (byte?)null,
                            EmployerIdentifierAtStartOfLearning = learner.LearnerEmploymentStatus
                                .Where(x => x.DateEmpStatApp <= payment.LearningStartDate)
                                .OrderByDescending(x => x.DateEmpStatApp).FirstOrDefault()?.EmpId,
                            ApplicableProgrammeStartDate = aecLearningDeliveryInfo?.AppAdjLearnStartDate,
                            TotalPMRPreviousFundingYears = prevYearAppFinData?.Sum(x => x.AFinCode == 1 || x.AFinCode == 2 ? x.AFinAmount : -x.AFinAmount) ?? 0,
                            TotalCoInvestmentDueFromEmployerInPreviousFundingYears = payment.PaymentInfoList.Where(x => x.FundingSource == _fundingSource &&
                                                                                                                        _transactionTypes.Any(y => y == x.TransactionType) &&
                                                                                                                        x.AcademicYear != Generics.AcademicYear).Sum(x => x.Amount),
                            TotalPMRThisFundingYear = currentYearAppFinData?.Sum(x => x.AFinCode == 1 || x.AFinCode == 2 ? x.AFinAmount : -x.AFinAmount) ?? 0,
                            LDM356Or361 = learningDelivery.LearningDeliveryFAMs.Any(
                                x => x.LearnDelFAMType.CaseInsensitiveEquals(Generics.LearningDeliveryFAMCodeLDM)
                                     && (x.LearnDelFAMCode.CaseInsensitiveEquals(Generics.LearningDeliveryFAMCode356)
                                         || x.LearnDelFAMCode.CaseInsensitiveEquals(Generics.LearningDeliveryFAMCode361))) ? "Yes" : "No",
                            CompletionEarningThisFundingYear = rulebaseInfo?.SelectMany(x => x.Periods).Sum() ?? 0,
                            CompletionPaymentsThisFundingYear = payment.PaymentInfoList.Where(x => x.TransactionType == 3 && x.AcademicYear == Generics.AcademicYear).Sum(x => x.Amount),
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
                                   (model.TotalCoInvestmentDueFromEmployerInPreviousFundingYears + model.TotalCoInvestmentDueFromEmployerThisFundingYear)) * 100;

                        var minSfaContributionPercentage = payment.PaymentInfoList.Where(x =>
                                x.FundingSource == _fundingSource && _transactionTypes.Any(y => y == x.TransactionType))
                            .GroupBy(x => x.DeliveryPeriod).Select(x => new
                            {
                                TotalAmount = x.Sum(y => y.Amount),
                                SfaContributionPercentage = x.Min(y => y.SfaContributionPercentage)
                            }).ToList().Where(x => x.TotalAmount != 0).OrderBy(x => x.SfaContributionPercentage)
                            .FirstOrDefault()?.SfaContributionPercentage ?? 0;

                        model.EmployerCoInvestmentPercentage = (1 - minSfaContributionPercentage) * 100;

                        model.EmployerNameFromApprenticeshipService = payment.PaymentInfoList
                            .OrderBy(x => x.DeliveryPeriod).FirstOrDefault()?.EmployerName;

                        appsCoInvestmentContributionsModels.Add(model);
                    }
                }
            }

            return appsCoInvestmentContributionsModels;
        }

        private decimal CalculateCoInvestmentDueForMonth(bool flag, IEnumerable<PaymentInfo> paymentInfoList, int deliveryPeriod)
        {
            return flag
                ? paymentInfoList.Where(x => x.DeliveryPeriod == deliveryPeriod).Sum(x => x.Amount)
                : 0;
        }
    }
}