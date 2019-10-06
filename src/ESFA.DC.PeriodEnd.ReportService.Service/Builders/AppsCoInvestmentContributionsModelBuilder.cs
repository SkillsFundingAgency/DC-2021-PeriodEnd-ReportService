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

        public IEnumerable<AppsCoInvestmentContributionsModel> BuildModelOld(
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

                if (ilrLearningDeliveriesInfo.Any() || rulebaseInfo.Any() ||
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
                            CompletionEarningThisFundingYear = rulebaseInfo.SelectMany(x => x.Periods).Sum() ?? 0,
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

        public decimal CalculateCoInvestmentDueForMonth(bool flag, IEnumerable<PaymentInfo> paymentInfoList, int deliveryPeriod)
        {
            return flag
                ? paymentInfoList.Where(x => x.DeliveryPeriod == deliveryPeriod).Sum(x => x.Amount)
                : 0;
        }

        //---------------------------------------------------------------------------------------------------------------------------------------------

        public IEnumerable<AppsCoInvestmentContributionsModel> BuildModel(
            AppsCoInvestmentILRInfo appsCoInvestmentIlrInfo,
            AppsCoInvestmentRulebaseInfo appsCoInvestmentRulebaseInfo,
            AppsCoInvestmentPaymentsInfo appsCoInvestmentPaymentsInfo)
        {
            if (appsCoInvestmentIlrInfo == null || appsCoInvestmentIlrInfo.Learners == null)
            {
                throw new Exception("Error: BuildModel() - AppsCoInvestmentILRInfo is null, no data has been retrieved from the ILR1920 data store.");
            }

            if (appsCoInvestmentRulebaseInfo == null)
            {
                throw new Exception("Error: BuildModel() - AppsCoInvestmentRulebaseInfo is null, no data has been retrieved from the ILR1920 data store.");
            }

            if (appsCoInvestmentPaymentsInfo == null)
            {
                throw new Exception("Error: BuildModel() - appsCoInvestmentPaymentsInfo is null, no data has been retrieved from the Payments data store.");
            }

            List<AppsCoInvestmentContributionsModel> appsCoInvestmentContributionsModels = new List<AppsCoInvestmentContributionsModel>();

            /*
             * Get a list of all the payments grouped by:
             *   •	LearnerReferenceNumber
             *   •	LearningStartDate
             *   •	LearningAimProgrammeType
             *   •	LearningAimStandardCode
             *   •	LearningAimFrameworkCode
             *   •	LearningAimPathwayCode
             */
            var paymentGroups = appsCoInvestmentPaymentsInfo.Payments
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
                })
                .OrderBy(x => x.LearnerReferenceNumber)
                .ThenBy(x => x.LearningStartDate)
                .ThenBy(x => x.LearningAimProgrammeType)
                .ThenBy(x => x.LearningAimStandardCode)
                .ThenBy(x => x.LearningAimFrameworkCode)
                .ThenBy(x => x.LearningAimPathwayCode).ToList();

            // Iterate through the payments groups calculating the payments and retrieving the related reference data
            foreach (var paymentGroup in paymentGroups)
            {
                // get the payment Programme Aim record associated with this payment (CoInvestment is only recorded against the Programme Aim)
                var payment = paymentGroup.PaymentInfoList.FirstOrDefault(x => x.LearningAimReference.CaseInsensitiveEquals(Generics.ZPROG001));

                if (payment != null)
                {
                    // get the ILR learner data associated with this payment
                    var learner = appsCoInvestmentIlrInfo.Learners.FirstOrDefault(x => x.LearnRefNumber.CaseInsensitiveEquals(payment.LearnerReferenceNumber));

                    if (learner != null)
                    {
                        // get the ILR learning delivery data associated with this payment
                        var ilrLearningDeliveriesInfos = learner.LearningDeliveries?.Where(x => x.AppFinRecords.Any(y =>
                            y.AFinType.CaseInsensitiveEquals(Generics.PMR) &&
                            y.LearnRefNumber.CaseInsensitiveEquals(learner.LearnRefNumber))).ToList();

                        if (ilrLearningDeliveriesInfos != null)
                        {
                            var learningDelivery = ilrLearningDeliveriesInfos.FirstOrDefault(x =>
                                x.UKPRN == payment.UkPrn &&
                                x.LearnRefNumber.CaseInsensitiveEquals(payment.LearnerReferenceNumber) &&
                                x.LearnAimRef.CaseInsensitiveEquals(payment.LearningAimReference) &&
                                x.LearnStartDate == payment.LearningStartDate &&
                                x.ProgType == payment.LearningAimProgrammeType &&
                                x.StdCode == payment.LearningAimProgrammeType &&
                                x.FworkCode == payment.LearningAimFrameworkCode &&
                                x.PwayCode == payment.LearningAimPathwayCode);

                            if (learningDelivery != null)
                            {
                                // Get the related App Fin data for previous years
                                var prevYearAppFinData = learningDelivery.AppFinRecords?
                                    .Where(x => x.AFinDate < Generics.BeginningOfYear &&
                                                x.AFinType.CaseInsensitiveEquals(Generics.PMR)).ToList();

                                // Get the related App Fin data for the current year
                                var currentYearAppFinData = learningDelivery.AppFinRecords?
                                    .Where(x => x.AFinDate >= Generics.BeginningOfYear &&
                                                x.AFinDate <= Generics.EndOfYear &&
                                                x.AFinType.CaseInsensitiveEquals(Generics.PMR)).ToList();

                                // Get the related AEC Learning Delivery data
                                var aecLearningDeliveryInfo =
                                    appsCoInvestmentRulebaseInfo.AECLearningDeliveries?.SingleOrDefault(x =>
                                        x.LearnRefNumber.CaseInsensitiveEquals(payment.LearnerReferenceNumber) &&
                                        x.AimSeqNumber == learningDelivery.AimSeqNumber);

                                // Check if this is a Maths or English aim
                                bool learnDelMathEng =
                                    aecLearningDeliveryInfo?.LearningDeliveryValues.LearnDelMathEng ?? false;

                                // Maths and English payments are fully funded by the ESFA so don't include them on the report
                                bool flagCalculateCoInvestmentAmount =
                                    !learnDelMathEng &&
                                    (payment.FundingSource == _fundingSource && _transactionTypes.Any(x => x == payment.TransactionType)) &&
                                    payment.AcademicYear == Generics.AcademicYear;

                                // get the AEC_ApprenticeshipPriceEpisodePeriodisedValues data associated with this payment
                                var rulebaseInfo = appsCoInvestmentRulebaseInfo.AECApprenticeshipPriceEpisodePeriodisedValues?.Where(x =>
                                    x.AttributeName.CaseInsensitiveEquals(_priceEpisodeCompletionPayment) &&
                                    x.Periods.All(p => p != decimal.Zero) &&
                                    x.LearnRefNumber.CaseInsensitiveEquals(learner.LearnRefNumber)).ToList();

                                var model = new AppsCoInvestmentContributionsModel
                                {
                                    LearnRefNumber = payment.LearnerReferenceNumber,
                                    UniqueLearnerNumber = paymentGroup.PaymentInfoList.Select(x => x.LearnerUln).Distinct().Count() == 1 ? payment.LearnerUln : (long?)null,
                                    LearningStartDate = payment.LearningStartDate?.ToString("dd/MM/yyyy"),
                                    ProgType = payment.LearningAimProgrammeType,
                                    StandardCode = payment.LearningAimStandardCode,
                                    FrameworkCode = payment.LearningAimFrameworkCode,
                                    ApprenticeshipPathway = payment.LearningAimPathwayCode,
                                    SoftwareSupplierAimIdentifier = learningDelivery.SWSupAimId ?? null,
                                    LearningDeliveryFAMTypeApprenticeshipContractType = !paymentGroup.PaymentInfoList.Select(x => x.ContractType).Distinct().Any() ? payment.ContractType : (byte?)null,
                                    EmployerIdentifierAtStartOfLearning = learner.LearnerEmploymentStatus.Where(x => x.DateEmpStatApp <= payment.LearningStartDate)
                                        .OrderByDescending(x => x.DateEmpStatApp).FirstOrDefault()?.EmpId,
                                    ApplicableProgrammeStartDate = aecLearningDeliveryInfo?.AppAdjLearnStartDate,
                                    TotalPMRPreviousFundingYears = prevYearAppFinData?.Sum(x => x.AFinCode == 1 || x.AFinCode == 2 ? x.AFinAmount : -x.AFinAmount) ?? 0,
                                    TotalCoInvestmentDueFromEmployerInPreviousFundingYears = paymentGroup.PaymentInfoList.Where(x => x.FundingSource == _fundingSource &&
                                        _transactionTypes.Any(y => y == x.TransactionType) && x.AcademicYear != Generics.AcademicYear).Sum(x => x.Amount),
                                    TotalPMRThisFundingYear = currentYearAppFinData?.Sum(x => x.AFinCode == 1 || x.AFinCode == 2 ? x.AFinAmount : -x.AFinAmount) ?? 0,
                                    LDM356Or361 = learningDelivery.LearningDeliveryFAMs.Any(
                                             x => x.LearnDelFAMType.CaseInsensitiveEquals(Generics.LearningDeliveryFAMCodeLDM) &&
                                             (x.LearnDelFAMCode.CaseInsensitiveEquals(Generics.LearningDeliveryFAMCode356) ||
                                             x.LearnDelFAMCode.CaseInsensitiveEquals(Generics.LearningDeliveryFAMCode361))) ? "Yes" : "No",
                                    CompletionEarningThisFundingYear = rulebaseInfo.SelectMany(x => x.Periods).Sum() ?? 0,
                                    CompletionPaymentsThisFundingYear = paymentGroup.PaymentInfoList.Where(x => x.TransactionType == 3 && x.AcademicYear == Generics.AcademicYear).Sum(x => x.Amount),
                                    CoInvestmentDueFromEmployerForAugust = CalculateCoInvestmentDueForMonth(flagCalculateCoInvestmentAmount, paymentGroup.PaymentInfoList, 1),
                                    CoInvestmentDueFromEmployerForSeptember = CalculateCoInvestmentDueForMonth(flagCalculateCoInvestmentAmount, paymentGroup.PaymentInfoList, 2),
                                    CoInvestmentDueFromEmployerForOctober = CalculateCoInvestmentDueForMonth(flagCalculateCoInvestmentAmount, paymentGroup.PaymentInfoList, 3),
                                    CoInvestmentDueFromEmployerForNovember = CalculateCoInvestmentDueForMonth(flagCalculateCoInvestmentAmount, paymentGroup.PaymentInfoList, 4),
                                    CoInvestmentDueFromEmployerForDecember = CalculateCoInvestmentDueForMonth(flagCalculateCoInvestmentAmount, paymentGroup.PaymentInfoList, 5),
                                    CoInvestmentDueFromEmployerForJanuary = CalculateCoInvestmentDueForMonth(flagCalculateCoInvestmentAmount, paymentGroup.PaymentInfoList, 6),
                                    CoInvestmentDueFromEmployerForFebruary = CalculateCoInvestmentDueForMonth(flagCalculateCoInvestmentAmount, paymentGroup.PaymentInfoList, 7),
                                    CoInvestmentDueFromEmployerForMarch = CalculateCoInvestmentDueForMonth(flagCalculateCoInvestmentAmount, paymentGroup.PaymentInfoList, 8),
                                    CoInvestmentDueFromEmployerForApril = CalculateCoInvestmentDueForMonth(flagCalculateCoInvestmentAmount, paymentGroup.PaymentInfoList, 9),
                                    CoInvestmentDueFromEmployerForMay = CalculateCoInvestmentDueForMonth(flagCalculateCoInvestmentAmount, paymentGroup.PaymentInfoList, 10),
                                    CoInvestmentDueFromEmployerForJune = CalculateCoInvestmentDueForMonth(flagCalculateCoInvestmentAmount, paymentGroup.PaymentInfoList, 11),
                                    CoInvestmentDueFromEmployerForJuly = CalculateCoInvestmentDueForMonth(flagCalculateCoInvestmentAmount, paymentGroup.PaymentInfoList, 12),
                                    CoInvestmentDueFromEmployerForR13 = CalculateCoInvestmentDueForMonth(flagCalculateCoInvestmentAmount, paymentGroup.PaymentInfoList, 13),
                                    CoInvestmentDueFromEmployerForR14 = CalculateCoInvestmentDueForMonth(flagCalculateCoInvestmentAmount, paymentGroup.PaymentInfoList, 14),
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

                                var minSfaContributionPercentage = paymentGroup.PaymentInfoList.Where(x =>
                                        x.FundingSource == _fundingSource && _transactionTypes.Any(y => y == x.TransactionType))
                                    .GroupBy(x => x.DeliveryPeriod).Select(x => new
                                    {
                                        TotalAmount = x.Sum(y => y.Amount),
                                        SfaContributionPercentage = x.Min(y => y.SfaContributionPercentage)
                                    }).ToList().Where(x => x.TotalAmount != 0).OrderBy(x => x.SfaContributionPercentage)
                                    .FirstOrDefault()?.SfaContributionPercentage ?? 0;

                                model.EmployerCoInvestmentPercentage = (1 - minSfaContributionPercentage) * 100;

                                model.EmployerNameFromApprenticeshipService = paymentGroup.PaymentInfoList
                                    .OrderBy(x => x.DeliveryPeriod).FirstOrDefault()?.EmployerName;

                                appsCoInvestmentContributionsModels.Add(model);
                            }
                        }
                    }
                }
            }

            return appsCoInvestmentContributionsModels;
        }
    }
}