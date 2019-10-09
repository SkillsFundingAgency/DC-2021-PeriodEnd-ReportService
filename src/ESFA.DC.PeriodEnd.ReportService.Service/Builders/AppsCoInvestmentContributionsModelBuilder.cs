using System;
using System.Collections.Generic;
using System.Linq;
using ESFA.DC.Logging.Interfaces;
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
        private readonly ILogger _logger;

        public AppsCoInvestmentContributionsModelBuilder(ILogger logger)
        {
            _logger = logger;
        }

        public IEnumerable<AppsCoInvestmentContributionsModel> BuildModel(
            AppsCoInvestmentILRInfo appsCoInvestmentIlrInfo,
            AppsCoInvestmentRulebaseInfo appsCoInvestmentRulebaseInfo,
            AppsCoInvestmentPaymentsInfo appsCoInvestmentPaymentsInfo,
            // add List of record Keys
            long jobId)
        {
            string errorMessage = string.Empty;

            if (appsCoInvestmentIlrInfo == null || appsCoInvestmentIlrInfo.Learners == null)
            {
                errorMessage = "Error: BuildModel() - AppsCoInvestmentILRInfo is null, no data has been retrieved from the ILR1920 data store.";
                _logger.LogInfo(errorMessage, jobIdOverride: jobId);

                throw new Exception(errorMessage);
            }

            if (appsCoInvestmentRulebaseInfo == null)
            {
                errorMessage = "Error: BuildModel() - AppsCoInvestmentRulebaseInfo is null, no data has been retrieved from the ILR1920 data store.";
                _logger.LogInfo(errorMessage, jobIdOverride: jobId);

                throw new Exception(errorMessage);
            }

            if (appsCoInvestmentPaymentsInfo == null)
            {
                errorMessage = "Error: BuildModel() - appsCoInvestmentPaymentsInfo is null, no data has been retrieved from the Payments data store.";
                _logger.LogInfo(errorMessage, jobIdOverride: jobId);

                throw new Exception(errorMessage);
            }

            List<AppsCoInvestmentContributionsModel> appsCoInvestmentContributionsModels = new List<AppsCoInvestmentContributionsModel>();

            List<AppsCoInvestmentRecordKey> recordKeys = new List<AppsCoInvestmentRecordKey>();

            var learnRefNumbers = GetRelevantLearners(appsCoInvestmentIlrInfo, appsCoInvestmentPaymentsInfo);

            var filteredRecordKeys = FilterRelevantLearnersFromPaymentsRecordKeys(learnRefNumbers, recordKeys);

            var filterReportRows = filteredRecordKeys.Where(r => FilterReportRows(appsCoInvestmentPaymentsInfo, appsCoInvestmentRulebaseInfo, appsCoInvestmentIlrInfo, r));

            foreach (var row in filterReportRows)
            {
            }

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
                    var model = new AppsCoInvestmentContributionsModel
                    {
                        LearnRefNumber = payment.LearnerReferenceNumber,
                        UniqueLearnerNumber = paymentGroup.PaymentInfoList.Select(x => x.LearnerUln).Distinct().Count() == 1 ? payment.LearnerUln : (long?)null,
                        LearningStartDate = payment.LearningStartDate?.ToString("dd/MM/yyyy"),
                        ProgType = payment.LearningAimProgrammeType,
                        StandardCode = payment.LearningAimStandardCode,
                        FrameworkCode = payment.LearningAimFrameworkCode,
                        ApprenticeshipPathway = payment.LearningAimPathwayCode,

                        CompletionPaymentsThisFundingYear = paymentGroup.PaymentInfoList.Where(x => x.TransactionType == 3 && x.AcademicYear == Generics.AcademicYear).Sum(x => x.Amount),
                        CoInvestmentDueFromEmployerForAugust = CalculateCoInvestmentDueForPeriod(true, paymentGroup.PaymentInfoList, 1),
                        CoInvestmentDueFromEmployerForSeptember = CalculateCoInvestmentDueForPeriod(true, paymentGroup.PaymentInfoList, 2),
                        CoInvestmentDueFromEmployerForOctober = CalculateCoInvestmentDueForPeriod(true, paymentGroup.PaymentInfoList, 3),
                        CoInvestmentDueFromEmployerForNovember = CalculateCoInvestmentDueForPeriod(true, paymentGroup.PaymentInfoList, 4),
                        CoInvestmentDueFromEmployerForDecember = CalculateCoInvestmentDueForPeriod(true, paymentGroup.PaymentInfoList, 5),
                        CoInvestmentDueFromEmployerForJanuary = CalculateCoInvestmentDueForPeriod(true, paymentGroup.PaymentInfoList, 6),
                        CoInvestmentDueFromEmployerForFebruary = CalculateCoInvestmentDueForPeriod(true, paymentGroup.PaymentInfoList, 7),
                        CoInvestmentDueFromEmployerForMarch = CalculateCoInvestmentDueForPeriod(true, paymentGroup.PaymentInfoList, 8),
                        CoInvestmentDueFromEmployerForApril = CalculateCoInvestmentDueForPeriod(true, paymentGroup.PaymentInfoList, 9),
                        CoInvestmentDueFromEmployerForMay = CalculateCoInvestmentDueForPeriod(true, paymentGroup.PaymentInfoList, 10),
                        CoInvestmentDueFromEmployerForJune = CalculateCoInvestmentDueForPeriod(true, paymentGroup.PaymentInfoList, 11),
                        CoInvestmentDueFromEmployerForJuly = CalculateCoInvestmentDueForPeriod(true, paymentGroup.PaymentInfoList, 12),
                        CoInvestmentDueFromEmployerForR13 = CalculateCoInvestmentDueForPeriod(true, paymentGroup.PaymentInfoList, 13),
                        CoInvestmentDueFromEmployerForR14 = CalculateCoInvestmentDueForPeriod(true, paymentGroup.PaymentInfoList, 14),
                    };

                    model.TotalCoInvestmentDueFromEmployerThisFundingYear =
                                    model.CoInvestmentDueFromEmployerForAugust + model.CoInvestmentDueFromEmployerForSeptember + model.CoInvestmentDueFromEmployerForOctober +
                                    model.CoInvestmentDueFromEmployerForNovember + model.CoInvestmentDueFromEmployerForDecember + model.CoInvestmentDueFromEmployerForJanuary +
                                    model.CoInvestmentDueFromEmployerForFebruary + model.CoInvestmentDueFromEmployerForMarch + model.CoInvestmentDueFromEmployerForApril +
                                    model.CoInvestmentDueFromEmployerForMay + model.CoInvestmentDueFromEmployerForJune + model.CoInvestmentDueFromEmployerForJuly +
                                    model.CoInvestmentDueFromEmployerForR13 + model.CoInvestmentDueFromEmployerForR14;

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
                                x.StdCode == payment.LearningAimStandardCode &&
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

                                model.SoftwareSupplierAimIdentifier = learningDelivery.SWSupAimId ?? null;
                                model.LearningDeliveryFAMTypeApprenticeshipContractType = !paymentGroup.PaymentInfoList.Select(x => x.ContractType).Distinct().Any() ? payment.ContractType : (byte?)null;
                                model.EmployerIdentifierAtStartOfLearning = learner.LearnerEmploymentStatus.Where(x => x.DateEmpStatApp <= payment.LearningStartDate)
                                         .OrderByDescending(x => x.DateEmpStatApp).FirstOrDefault()?.EmpId;
                                model.ApplicableProgrammeStartDate = aecLearningDeliveryInfo?.AppAdjLearnStartDate;
                                model.TotalPMRPreviousFundingYears = prevYearAppFinData?.Sum(x => x.AFinCode == 1 || x.AFinCode == 2 ? x.AFinAmount : -x.AFinAmount) ?? 0;
                                model.TotalCoInvestmentDueFromEmployerInPreviousFundingYears = paymentGroup.PaymentInfoList.Where(x => x.FundingSource == _fundingSource &&
                                       _transactionTypes.Any(y => y == x.TransactionType) && x.AcademicYear != Generics.AcademicYear).Sum(x => x.Amount);
                                model.TotalPMRThisFundingYear = currentYearAppFinData?.Sum(x => x.AFinCode == 1 || x.AFinCode == 2 ? x.AFinAmount : -x.AFinAmount) ?? 0;
                                model.LDM356Or361 = learningDelivery.LearningDeliveryFAMs.Any(
                                              x => x.LearnDelFAMType.CaseInsensitiveEquals(Generics.LearningDeliveryFAMCodeLDM) &&
                                              (x.LearnDelFAMCode.CaseInsensitiveEquals(Generics.LearningDeliveryFAMCode356) ||
                                              x.LearnDelFAMCode.CaseInsensitiveEquals(Generics.LearningDeliveryFAMCode361))) ? "Yes" : "No";

                                //var model1 = new AppsCoInvestmentContributionsModel
                                //{
                                //    LearnRefNumber = payment.LearnerReferenceNumber,
                                //    UniqueLearnerNumber = paymentGroup.PaymentInfoList.Select(x => x.LearnerUln).Distinct().Count() == 1 ? payment.LearnerUln : (long?)null,
                                //    LearningStartDate = payment.LearningStartDate?.ToString("dd/MM/yyyy"),
                                //    ProgType = payment.LearningAimProgrammeType,
                                //    StandardCode = payment.LearningAimStandardCode,
                                //    FrameworkCode = payment.LearningAimFrameworkCode,
                                //    ApprenticeshipPathway = payment.LearningAimPathwayCode,
                                //    SoftwareSupplierAimIdentifier = learningDelivery.SWSupAimId ?? null,
                                //    LearningDeliveryFAMTypeApprenticeshipContractType = !paymentGroup.PaymentInfoList.Select(x => x.ContractType).Distinct().Any() ? payment.ContractType : (byte?)null,
                                //    EmployerIdentifierAtStartOfLearning = learner.LearnerEmploymentStatus.Where(x => x.DateEmpStatApp <= payment.LearningStartDate)
                                //        .OrderByDescending(x => x.DateEmpStatApp).FirstOrDefault()?.EmpId,
                                //    ApplicableProgrammeStartDate = aecLearningDeliveryInfo?.AppAdjLearnStartDate,
                                //    TotalPMRPreviousFundingYears = prevYearAppFinData?.Sum(x => x.AFinCode == 1 || x.AFinCode == 2 ? x.AFinAmount : -x.AFinAmount) ?? 0,
                                //    TotalCoInvestmentDueFromEmployerInPreviousFundingYears = paymentGroup.PaymentInfoList.Where(x => x.FundingSource == _fundingSource &&
                                //        _transactionTypes.Any(y => y == x.TransactionType) && x.AcademicYear != Generics.AcademicYear).Sum(x => x.Amount),
                                //    TotalPMRThisFundingYear = currentYearAppFinData?.Sum(x => x.AFinCode == 1 || x.AFinCode == 2 ? x.AFinAmount : -x.AFinAmount) ?? 0,
                                //    LDM356Or361 = learningDelivery.LearningDeliveryFAMs.Any(
                                //             x => x.LearnDelFAMType.CaseInsensitiveEquals(Generics.LearningDeliveryFAMCodeLDM) &&
                                //             (x.LearnDelFAMCode.CaseInsensitiveEquals(Generics.LearningDeliveryFAMCode356) ||
                                //             x.LearnDelFAMCode.CaseInsensitiveEquals(Generics.LearningDeliveryFAMCode361))) ? "Yes" : "No",
                                //    CompletionEarningThisFundingYear = rulebaseInfo.SelectMany(x => x.Periods).Sum() ?? 0,
                                //    CompletionPaymentsThisFundingYear = paymentGroup.PaymentInfoList.Where(x => x.TransactionType == 3 && x.AcademicYear == Generics.AcademicYear).Sum(x => x.Amount),
                                //    CoInvestmentDueFromEmployerForAugust = CalculateCoInvestmentDueForPeriod(flagCalculateCoInvestmentAmount, paymentGroup.PaymentInfoList, 1),
                                //    CoInvestmentDueFromEmployerForSeptember = CalculateCoInvestmentDueForPeriod(flagCalculateCoInvestmentAmount, paymentGroup.PaymentInfoList, 2),
                                //    CoInvestmentDueFromEmployerForOctober = CalculateCoInvestmentDueForPeriod(flagCalculateCoInvestmentAmount, paymentGroup.PaymentInfoList, 3),
                                //    CoInvestmentDueFromEmployerForNovember = CalculateCoInvestmentDueForPeriod(flagCalculateCoInvestmentAmount, paymentGroup.PaymentInfoList, 4),
                                //    CoInvestmentDueFromEmployerForDecember = CalculateCoInvestmentDueForPeriod(flagCalculateCoInvestmentAmount, paymentGroup.PaymentInfoList, 5),
                                //    CoInvestmentDueFromEmployerForJanuary = CalculateCoInvestmentDueForPeriod(flagCalculateCoInvestmentAmount, paymentGroup.PaymentInfoList, 6),
                                //    CoInvestmentDueFromEmployerForFebruary = CalculateCoInvestmentDueForPeriod(flagCalculateCoInvestmentAmount, paymentGroup.PaymentInfoList, 7),
                                //    CoInvestmentDueFromEmployerForMarch = CalculateCoInvestmentDueForPeriod(flagCalculateCoInvestmentAmount, paymentGroup.PaymentInfoList, 8),
                                //    CoInvestmentDueFromEmployerForApril = CalculateCoInvestmentDueForPeriod(flagCalculateCoInvestmentAmount, paymentGroup.PaymentInfoList, 9),
                                //    CoInvestmentDueFromEmployerForMay = CalculateCoInvestmentDueForPeriod(flagCalculateCoInvestmentAmount, paymentGroup.PaymentInfoList, 10),
                                //    CoInvestmentDueFromEmployerForJune = CalculateCoInvestmentDueForPeriod(flagCalculateCoInvestmentAmount, paymentGroup.PaymentInfoList, 11),
                                //    CoInvestmentDueFromEmployerForJuly = CalculateCoInvestmentDueForPeriod(flagCalculateCoInvestmentAmount, paymentGroup.PaymentInfoList, 12),
                                //    CoInvestmentDueFromEmployerForR13 = CalculateCoInvestmentDueForPeriod(flagCalculateCoInvestmentAmount, paymentGroup.PaymentInfoList, 13),
                                //    CoInvestmentDueFromEmployerForR14 = CalculateCoInvestmentDueForPeriod(flagCalculateCoInvestmentAmount, paymentGroup.PaymentInfoList, 14),
                                //};

                                //model.TotalCoInvestmentDueFromEmployerThisFundingYear =
                                //    model.CoInvestmentDueFromEmployerForAugust + model.CoInvestmentDueFromEmployerForSeptember + model.CoInvestmentDueFromEmployerForOctober +
                                //    model.CoInvestmentDueFromEmployerForNovember + model.CoInvestmentDueFromEmployerForDecember + model.CoInvestmentDueFromEmployerForJanuary +
                                //    model.CoInvestmentDueFromEmployerForFebruary + model.CoInvestmentDueFromEmployerForMarch + model.CoInvestmentDueFromEmployerForApril +
                                //    model.CoInvestmentDueFromEmployerForMay + model.CoInvestmentDueFromEmployerForJune + model.CoInvestmentDueFromEmployerForJuly +
                                //    model.CoInvestmentDueFromEmployerForR13 + model.CoInvestmentDueFromEmployerForR14;

                                model.PercentageOfCoInvestmentCollected =
                                    (int)(model.TotalCoInvestmentDueFromEmployerInPreviousFundingYears +
                                    model.TotalCoInvestmentDueFromEmployerThisFundingYear == 0
                                        ? 0
                                        : ((model.TotalPMRPreviousFundingYears + model.TotalPMRThisFundingYear) /
                                           (model.TotalCoInvestmentDueFromEmployerInPreviousFundingYears + model.TotalCoInvestmentDueFromEmployerThisFundingYear)) * 100);

                                //var minSfaContributionPercentage = paymentGroup.PaymentInfoList.Where(x =>
                                //        x.FundingSource == _fundingSource && _transactionTypes.Any(y => y == x.TransactionType))
                                //    .GroupBy(x => x.DeliveryPeriod).Select(x => new
                                //    {
                                //        TotalAmount = x.Sum(y => y.Amount),
                                //        SfaContributionPercentage = x.Min(y => y.SfaContributionPercentage)
                                //    }).ToList().Where(x => x.TotalAmount != 0).OrderBy(x => x.SfaContributionPercentage)
                                //    .FirstOrDefault()?.SfaContributionPercentage ?? 0;

                                //model.EmployerCoInvestmentPercentage = (1 - minSfaContributionPercentage) * 100;

                                //model.EmployerNameFromApprenticeshipService = paymentGroup.PaymentInfoList
                                //    .OrderBy(x => x.DeliveryPeriod).FirstOrDefault()?.EmployerName;
                            }
                        }
                    }

                    appsCoInvestmentContributionsModels.Add(model);
                }
            }

            return appsCoInvestmentContributionsModels;
        }

        public decimal CalculateCoInvestmentDueForPeriod(bool flag, IEnumerable<PaymentInfo> paymentInfoList, int collectionPeriod)
        {
            return flag
                ? paymentInfoList.Where(x => x.CollectionPeriod == collectionPeriod).Sum(x => x.Amount)
                : 0;
        }

        // BR1
        public IEnumerable<string> GetRelevantLearners(AppsCoInvestmentILRInfo ilrInfo, AppsCoInvestmentPaymentsInfo paymentsInfo)
        {
            var ilrLearnRefNumbers = ilrInfo
                .Learners?
                .Where(l =>
                    l.LearningDeliveries?
                        .Any(ld => ld.FundModel == 36
                        && (ld.AppFinRecords?.Any(afr => afr.AFinType == "PMR")
                        ?? false))
                    ?? false)
                .Select(l => l.LearnRefNumber).ToList()
                ?? Enumerable.Empty<string>();

            var paymentLearnRefNumbers = paymentsInfo
                .Payments
                .Where(p => p.FundingSource == _fundingSource)
                .Select(p => p.LearnerReferenceNumber).ToList();

            return ilrLearnRefNumbers.Union(paymentLearnRefNumbers);
        }

        public IEnumerable<AppsCoInvestmentRecordKey> FilterRelevantLearnersFromPaymentsRecordKeys(IEnumerable<string> learnRefNumbers, IEnumerable<AppsCoInvestmentRecordKey> recordKeys)
        {
            var learnRefNumbersHashSet = new HashSet<string>(learnRefNumbers);

            return recordKeys.Where(r => learnRefNumbersHashSet.Contains(r.LearnerReferenceNumber));
        }

        // BR2
        public bool FilterReportRows(AppsCoInvestmentPaymentsInfo paymentInfo, AppsCoInvestmentRulebaseInfo rulebaseInfo, AppsCoInvestmentILRInfo ilrInfo, AppsCoInvestmentRecordKey recordKey)
        {
            return
                EmployerCoInvestmentPaymentFilter(paymentInfo, recordKey.LearnerReferenceNumber)
                || CompletionPaymentFilter(paymentInfo, recordKey.LearnerReferenceNumber)
                || PMRAppFinRecordFilter(ilrInfo, recordKey.LearnerReferenceNumber)
                || NonZeroCompletionEarningsFilter(rulebaseInfo, recordKey.LearnerReferenceNumber);
        }

        public bool CompletionPaymentFilter(AppsCoInvestmentPaymentsInfo paymentsInfo, string learnRefNumber)
        {
            return paymentsInfo.Payments?.Any(p => p.TransactionType == 3 && p.LearnerReferenceNumber.CaseInsensitiveEquals(learnRefNumber)) ?? false;
        }

        public bool EmployerCoInvestmentPaymentFilter(AppsCoInvestmentPaymentsInfo paymentsInfo, string learnRefNumber)
        {
            return paymentsInfo.Payments?.Any(p => p.FundingSource == 3 && p.LearnerReferenceNumber.CaseInsensitiveEquals(learnRefNumber)) ?? false;
        }

        public bool NonZeroCompletionEarningsFilter(AppsCoInvestmentRulebaseInfo rulebaseInfo, string learnRefNumber)
        {
            return rulebaseInfo.AECApprenticeshipPriceEpisodePeriodisedValues?
                .Any(
                    p =>
                        p.AttributeName == "PriceEpisodeCompletionPayment"
                        && (p.Periods?.Any(v => v.HasValue && v != 0) ?? false))
                ?? false;
        }

        public bool PMRAppFinRecordFilter(AppsCoInvestmentILRInfo ilrInfo, string learnRefNumber)
        {
            return ilrInfo
                .Learners?.Any(
                l =>
                    l.LearnRefNumber.CaseInsensitiveEquals(learnRefNumber)
                    && (l.LearningDeliveries?.Any(ld =>
                        ld.AppFinRecords?.Any(afr => afr.AFinType.CaseInsensitiveEquals(Generics.PMR))
                        ?? false)
                    ?? false))
                ?? false;
        }
    }
}