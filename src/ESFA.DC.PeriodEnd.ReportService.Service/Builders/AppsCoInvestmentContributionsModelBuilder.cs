using System;
using System.Collections.Generic;
using System.Linq;
using ESFA.DC.Logging.Interfaces;
using ESFA.DC.PeriodEnd.ReportService.Interface.Builders;
using ESFA.DC.PeriodEnd.ReportService.Model.PeriodEnd.AppsCoInvestment;
using ESFA.DC.PeriodEnd.ReportService.Model.PeriodEnd.Common;
using ESFA.DC.PeriodEnd.ReportService.Service.Constants;
using ESFA.DC.PeriodEnd.ReportService.Service.Extensions;

namespace ESFA.DC.PeriodEnd.ReportService.Service.Builders
{
    public class AppsCoInvestmentContributionsModelBuilder : IAppsCoInvestmentContributionsModelBuilder
    {
        private const int _fundingSource = 3;
        private readonly HashSet<int> _transactionTypes = new HashSet<int>()
        {
            Constants.DASPayments.TransactionType.Learning_On_Programme,
            Constants.DASPayments.TransactionType.Completion,
            Constants.DASPayments.TransactionType.Balancing,
        };

        private readonly DateTime _academicYearStart = new DateTime(2019, 8, 1);
        private readonly DateTime _nextAcademicYearStart = new DateTime(2020, 8, 1);

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
            List<AppsCoInvestmentRecordKey> paymentsAppsCoInvestmentUniqueKeys,
            List<AppsCoInvestmentRecordKey> ilrAppsCoInvestmentUniqueKeys,
            IDictionary<long, string> apprenticeshipIdLegalEntityNameDictionary,
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

            var learnRefNumbers = GetRelevantLearners(appsCoInvestmentIlrInfo, appsCoInvestmentPaymentsInfo).ToList();

            var uniqueKeys = UnionKeys(ilrAppsCoInvestmentUniqueKeys, paymentsAppsCoInvestmentUniqueKeys).ToList();

            var filteredRecordKeys = FilterRelevantLearnersFromPaymentsRecordKeys(learnRefNumbers, uniqueKeys).ToList();

            var filterReportRows = filteredRecordKeys.Where(r => FilterReportRows(appsCoInvestmentPaymentsInfo, appsCoInvestmentRulebaseInfo, appsCoInvestmentIlrInfo, r)).ToList();

            return filterReportRows
                .Select(record =>
                {
                    var paymentRecords = GetPaymentInfosForRecord(appsCoInvestmentPaymentsInfo, record).ToList();
                    var learner = GetLearnerForRecord(appsCoInvestmentIlrInfo, record);
                    var learningDelivery = GetLearningDeliveryForRecord(learner, record);
                    var filteredPaymentRecords = FundingSourceAndTransactionTypeFilter(paymentRecords).ToList();
                    var rulebaseLearningDelivery = GetRulebaseLearningDelivery(appsCoInvestmentRulebaseInfo, learningDelivery);
                    var isEarliestStartDate = IsEarliestLearningStartDate(filteredRecordKeys, record);
                    var completedPaymentRecordsInCurrentYear = paymentRecords?.Where(p => p.AcademicYear == Generics.AcademicYear && p.TransactionType == 2).ToList();
                    var totalsByPeriodDictionary = BuildCoinvestmentPaymentsPerPeriodDictionary(filteredPaymentRecords);
                    var earliestPaymentInfo = GetEarliestPaymentInfo(paymentRecords);

                    string legalEntityName = null;

                    if (earliestPaymentInfo != null && earliestPaymentInfo.ApprenticeshipId.HasValue)
                    {
                        apprenticeshipIdLegalEntityNameDictionary.TryGetValue(earliestPaymentInfo.ApprenticeshipId.Value, out legalEntityName);
                    }

                    var totalDueCurrentYear = totalsByPeriodDictionary.Sum(d => d.Value);
                    var totalDuePreviousYear = isEarliestStartDate ? filteredPaymentRecords?.Where(p => p.AcademicYear < 1920).Sum(p => p.Amount) : null;
                    var totalCollectedCurrentYear = isEarliestStartDate ? GetTotalPMRBetweenDates(learningDelivery, _academicYearStart, _nextAcademicYearStart) : null;
                    var totalCollectedPreviousYear = isEarliestStartDate ? GetTotalPMRBetweenDates(learningDelivery, null, _academicYearStart) : null;

                    var model = new AppsCoInvestmentContributionsModel
                    {
                        LearnRefNumber = record.LearnerReferenceNumber,
                        UniqueLearnerNumber = GetUniqueOrEmpty(paymentRecords, p => p.LearnerUln),
                        LearningStartDate = record.LearningStartDate,
                        ProgType = record.LearningAimProgrammeType,
                        StandardCode = record.LearningAimStandardCode,
                        FrameworkCode = record.LearningAimFrameworkCode,
                        ApprenticeshipPathway = record.LearningAimPathwayCode,
                        SoftwareSupplierAimIdentifier = learningDelivery?.SWSupAimId,
                        LearningDeliveryFAMTypeApprenticeshipContractType = GetUniqueOrEmpty(paymentRecords, p => p.ContractType),
                        EmployerIdentifierAtStartOfLearning = learner?.LearnerEmploymentStatus.Where(w => w.DateEmpStatApp <= record.LearningStartDate).OrderByDescending(o => o.DateEmpStatApp).FirstOrDefault()?.EmpId,
                        EmployerNameFromApprenticeshipService = legalEntityName,
                        EmployerCoInvestmentPercentage = GetEmployerCoInvestmentPercentage(filteredPaymentRecords),
                        ApplicableProgrammeStartDate = rulebaseLearningDelivery?.AppAdjLearnStartDate,
                        TotalPMRPreviousFundingYears = totalCollectedPreviousYear,
                        TotalCoInvestmentDueFromEmployerInPreviousFundingYears = totalDuePreviousYear,
                        TotalPMRThisFundingYear = totalCollectedCurrentYear,
                        TotalCoInvestmentDueFromEmployerThisFundingYear = totalDueCurrentYear,
                        PercentageOfCoInvestmentCollected = GetPercentageOfInvestmentCollected(totalDueCurrentYear, totalDuePreviousYear, totalCollectedCurrentYear, totalCollectedPreviousYear),
                        LDM356Or361 = HasLdm356Or361(learningDelivery) ? "Yes" : "No",
                        CompletionEarningThisFundingYear = CalculateCompletionEarningsThisFundingYear(appsCoInvestmentRulebaseInfo, paymentRecords),
                        CompletionPaymentsThisFundingYear = completedPaymentRecordsInCurrentYear.Sum(r => r.Amount),
                        CoInvestmentDueFromEmployerForAugust = GetPeriodisedValueFromDictionaryForPeriod(totalsByPeriodDictionary, 1),
                        CoInvestmentDueFromEmployerForSeptember = GetPeriodisedValueFromDictionaryForPeriod(totalsByPeriodDictionary, 2),
                        CoInvestmentDueFromEmployerForOctober = GetPeriodisedValueFromDictionaryForPeriod(totalsByPeriodDictionary, 3),
                        CoInvestmentDueFromEmployerForNovember = GetPeriodisedValueFromDictionaryForPeriod(totalsByPeriodDictionary, 4),
                        CoInvestmentDueFromEmployerForDecember = GetPeriodisedValueFromDictionaryForPeriod(totalsByPeriodDictionary, 5),
                        CoInvestmentDueFromEmployerForJanuary = GetPeriodisedValueFromDictionaryForPeriod(totalsByPeriodDictionary, 6),
                        CoInvestmentDueFromEmployerForFebruary = GetPeriodisedValueFromDictionaryForPeriod(totalsByPeriodDictionary, 7),
                        CoInvestmentDueFromEmployerForMarch = GetPeriodisedValueFromDictionaryForPeriod(totalsByPeriodDictionary, 8),
                        CoInvestmentDueFromEmployerForApril = GetPeriodisedValueFromDictionaryForPeriod(totalsByPeriodDictionary, 9),
                        CoInvestmentDueFromEmployerForMay = GetPeriodisedValueFromDictionaryForPeriod(totalsByPeriodDictionary, 10),
                        CoInvestmentDueFromEmployerForJune = GetPeriodisedValueFromDictionaryForPeriod(totalsByPeriodDictionary, 11),
                        CoInvestmentDueFromEmployerForJuly = GetPeriodisedValueFromDictionaryForPeriod(totalsByPeriodDictionary, 12),
                        CoInvestmentDueFromEmployerForR13 = GetPeriodisedValueFromDictionaryForPeriod(totalsByPeriodDictionary, 13),
                        CoInvestmentDueFromEmployerForR14 = GetPeriodisedValueFromDictionaryForPeriod(totalsByPeriodDictionary, 14)
                    };

                    return model;
                })
                .OrderBy(l => l.LearnRefNumber)
                .ThenBy(t => t.LearningDeliveryFAMTypeApprenticeshipContractType);
        }

        public IEnumerable<AppsCoInvestmentRecordKey> UnionKeys(ICollection<AppsCoInvestmentRecordKey> ilrRecords, ICollection<AppsCoInvestmentRecordKey> paymentsRecords)
        {
            return ilrRecords.Union(paymentsRecords);
        }

        public decimal GetPercentageOfInvestmentCollected(decimal? totalDueCurrentYear, decimal? totalDuePreviousYear, decimal? totalCollectedCurrentYear, decimal? totalCollectedPreviousYear)
        {
            var totalDue = totalDuePreviousYear ?? 0 + totalDueCurrentYear ?? 0;

            if (totalDue == 0)
            {
                return 0;
            }

            var totalCollected = totalCollectedPreviousYear ?? 0 + totalCollectedCurrentYear ?? 0;

            return (totalCollected / totalDue) * 100;
        }

        public decimal GetPeriodisedValueFromDictionaryForPeriod(IDictionary<byte, decimal> periodisedDictionary, byte period)
        {
            if (periodisedDictionary.TryGetValue(period, out decimal value))
            {
                return value;
            }

            return decimal.Zero;
        }

        public Dictionary<byte, decimal> BuildCoinvestmentPaymentsPerPeriodDictionary(IEnumerable<PaymentInfo> paymentInfos)
        {
            return paymentInfos?
                .Where(p => p.AcademicYear == Generics.AcademicYear)
                .GroupBy(p => p.CollectionPeriod)
                .ToDictionary(p => p.Key, p => p.Sum(i => i.Amount));
        }

        public decimal CalculateCompletionEarningsThisFundingYear(AppsCoInvestmentRulebaseInfo rulebaseInfo, IEnumerable<PaymentInfo> paymentInfos)
        {
            var priceEpisodeIdentifiers = new HashSet<string>(paymentInfos?.Select(p => p.PriceEpisodeIdentifier) ?? Enumerable.Empty<string>());

            return rulebaseInfo?
                .AECApprenticeshipPriceEpisodePeriodisedValues?
                .Where(p => priceEpisodeIdentifiers.Contains(p.PriceEpisodeIdentifier))
                .Where(p => p.Periods != null)
                .SelectMany(p => p.Periods)
                .Sum()
                ?? 0;
        }

        public bool HasLdm356Or361(LearningDeliveryInfo learningDelivery)
        {
            return learningDelivery?
                .LearningDeliveryFAMs?
                .Any(
                    fam =>
                    fam.LearnDelFAMType.CaseInsensitiveEquals(Generics.LearningDeliveryFAMCodeLDM)
                    && (fam.LearnDelFAMCode == Generics.LearningDeliveryFAMCode356 || fam.LearnDelFAMCode == Generics.LearningDeliveryFAMCode361))
            ?? false;
        }

        public decimal? GetTotalPMRBetweenDates(LearningDeliveryInfo learningDelivery, DateTime? startDate, DateTime? endDate)
        {
            var pmrsQuery = learningDelivery?.AppFinRecords ?? Enumerable.Empty<AppFinRecordInfo>();

            if (startDate.HasValue)
            {
                pmrsQuery = pmrsQuery.Where(r => r.AFinDate >= startDate);
            }

            if (endDate.HasValue)
            {
                pmrsQuery = pmrsQuery.Where(r => r.AFinDate < endDate);
            }

            pmrsQuery = pmrsQuery.Where(r => r.AFinType.CaseInsensitiveEquals(Generics.PMR));

            var pmrs = pmrsQuery.ToList();

            var positive = pmrs.Where(p => p.AFinCode == 1 || p.AFinCode == 2).Sum(p => p.AFinAmount);
            var negative = pmrs.Where(p => p.AFinCode == 3).Sum(p => p.AFinAmount);

            return positive - negative;
        }

        public bool IsEarliestLearningStartDate(IEnumerable<AppsCoInvestmentRecordKey> recordKeys, AppsCoInvestmentRecordKey recordKey)
        {
            return recordKeys
                .Where(r =>
                    r.LearnerReferenceNumber.CaseInsensitiveEquals(recordKey.LearnerReferenceNumber)
                    && r.LearningAimProgrammeType == recordKey.LearningAimProgrammeType
                    && r.LearningAimStandardCode == recordKey.LearningAimStandardCode
                    && r.LearningAimFrameworkCode == recordKey.LearningAimFrameworkCode
                    && r.LearningAimPathwayCode == recordKey.LearningAimPathwayCode)
                    .Min(r => r.LearningStartDate) == recordKey.LearningStartDate;
        }

        public decimal GetEmployerCoInvestmentPercentage(IEnumerable<PaymentInfo> paymentInfos)
        {
            return (1 - paymentInfos
                        .GroupBy(g => new { g.DeliveryPeriod, g.AcademicYear })
                        .Select(s => new { AggAmount = s.Sum(a => a.Amount), SFaContrib = s.Min(m => m.SfaContributionPercentage) })
                        .Where(w => w.AggAmount != 0)
                        .Select(s => s.SFaContrib)
                        .DefaultIfEmpty()
                        .Min())
                        * 100;
        }

        public IEnumerable<PaymentInfo> FundingSourceAndTransactionTypeFilter(IEnumerable<PaymentInfo> paymentInfos)
        {
            return paymentInfos.Where(p => p.FundingSource == _fundingSource && _transactionTypes.Contains(p.TransactionType));
        }

        public PaymentInfo GetEarliestPaymentInfo(IEnumerable<PaymentInfo> paymentInfos)
        {
            return paymentInfos?
                .OrderBy(p => p.AcademicYear)
                .ThenBy(p => p.DeliveryPeriod)
                .ThenBy(p => p.CollectionPeriod)
                .FirstOrDefault();
        }

        public AECLearningDeliveryInfo GetRulebaseLearningDelivery(AppsCoInvestmentRulebaseInfo rulebaseInfo, LearningDeliveryInfo learningDelivery)
        {
            if (learningDelivery == null)
            {
                return null;
            }

            return rulebaseInfo
                .AECLearningDeliveries
                .FirstOrDefault(ld => ld.LearnRefNumber.CaseInsensitiveEquals(learningDelivery.LearnRefNumber) && ld.AimSeqNumber == learningDelivery.AimSeqNumber);
        }

        public IEnumerable<PaymentInfo> GetPaymentInfosForRecord(AppsCoInvestmentPaymentsInfo paymentsInfo, AppsCoInvestmentRecordKey record)
        {
            return paymentsInfo
                .Payments?
                .Where(p =>
                    p.LearnerReferenceNumber.CaseInsensitiveEquals(record.LearnerReferenceNumber)
                    && p.LearningStartDate == record.LearningStartDate
                    && p.LearningAimProgrammeType == record.LearningAimProgrammeType
                    && p.LearningAimStandardCode == record.LearningAimStandardCode
                    && p.LearningAimFrameworkCode == record.LearningAimFrameworkCode
                    && p.LearningAimPathwayCode == record.LearningAimPathwayCode
                    && p.LearningAimReference.CaseInsensitiveEquals(record.LearningAimReference))
                ?? Enumerable.Empty<PaymentInfo>();
        }

        public LearningDeliveryInfo GetLearningDeliveryForRecord(LearnerInfo learner, AppsCoInvestmentRecordKey record)
        {
            return learner?
                .LearningDeliveries
                .FirstOrDefault(ld => IlrLearningDeliveryRecordMatch(ld, record));
        }

        public LearnerInfo GetLearnerForRecord(AppsCoInvestmentILRInfo ilrInfo, AppsCoInvestmentRecordKey record)
        {
            return ilrInfo
                .Learners?
                .FirstOrDefault(l => l.LearnRefNumber.CaseInsensitiveEquals(record.LearnerReferenceNumber)
                    && (l.LearningDeliveries?.Any(ld => IlrLearningDeliveryRecordMatch(ld, record)) ?? false));
        }

        public bool IlrLearningDeliveryRecordMatch(LearningDeliveryInfo learningDelivery, AppsCoInvestmentRecordKey record)
        {
            return learningDelivery.LearnStartDate == record.LearningStartDate
                    && learningDelivery.ProgType == record.LearningAimProgrammeType
                    && learningDelivery.StdCode == record.LearningAimStandardCode
                    && learningDelivery.FworkCode == record.LearningAimFrameworkCode
                    && learningDelivery.PwayCode == record.LearningAimPathwayCode
                    && learningDelivery.LearnAimRef.CaseInsensitiveEquals(record.LearningAimReference);
        }

        public T? GetUniqueOrEmpty<TIn, T>(IEnumerable<TIn> input, Func<TIn, T> selector)
            where T : struct
        {
            var distinct = input.Select(selector).Distinct().ToList();

            if (distinct.Count > 1 || distinct.Count == 0)
            {
                return null;
            }

            return distinct.FirstOrDefault();
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