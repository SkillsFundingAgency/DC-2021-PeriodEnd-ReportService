using System;
using System.Collections.Generic;
using System.Linq;
using ESFA.DC.Logging.Interfaces;
using ESFA.DC.PeriodEnd.ReportService.Interface.Builders;
using ESFA.DC.PeriodEnd.ReportService.Model.PeriodEnd.AppsCoInvestment;
using ESFA.DC.PeriodEnd.ReportService.Model.PeriodEnd.AppsCoInvestment.Comparer;
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
        private readonly IEqualityComparer<AppsCoInvestmentRecordKey> _appsCoInvestmentEqualityComparer;

        public AppsCoInvestmentContributionsModelBuilder(IEqualityComparer<AppsCoInvestmentRecordKey> appsCoInvestmentEqualityComparer, ILogger logger)
        {
            _appsCoInvestmentEqualityComparer = appsCoInvestmentEqualityComparer;
            _logger = logger;
        }

        public IEnumerable<AppsCoInvestmentContributionsModel> BuildModel(
            AppsCoInvestmentILRInfo appsCoInvestmentIlrInfo,
            AppsCoInvestmentRulebaseInfo appsCoInvestmentRulebaseInfo,
            AppsCoInvestmentPaymentsInfo appsCoInvestmentPaymentsInfo,
            List<AppsCoInvestmentRecordKey> paymentsAppsCoInvestmentUniqueKeys,
            List<AppsCoInvestmentRecordKey> ilrAppsCoInvestmentUniqueKeys,
            IDictionary<long, string> apprenticeshipIdLegalEntityNameDictionary,
            long jobId,
            int ukprn)
        {
            string errorMessage;

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

            var paymentsDictionary = BuildPaymentInfoDictionary(appsCoInvestmentPaymentsInfo);
            var learnerDictionary = BuildLearnerDictionary(appsCoInvestmentIlrInfo);

            var relevantLearnRefNumbers = GetRelevantLearners(appsCoInvestmentIlrInfo, appsCoInvestmentPaymentsInfo).ToList();

            var uniqueKeys = UnionKeys(relevantLearnRefNumbers, ilrAppsCoInvestmentUniqueKeys, paymentsAppsCoInvestmentUniqueKeys).ToList();

            return uniqueKeys
                .Where(r => FilterReportRows(appsCoInvestmentPaymentsInfo, appsCoInvestmentRulebaseInfo, appsCoInvestmentIlrInfo, r))
                .Select(record =>
                {
                    var paymentRecords = GetPaymentInfosForRecord(paymentsDictionary, record).ToList();
                    var learner = GetLearnerForRecord(learnerDictionary, record);
                    var learningDelivery = GetLearningDeliveryForRecord(learner, record);
                    var filteredPaymentRecords = FundingSourceAndTransactionTypeFilter(paymentRecords).ToList();
                    var rulebaseLearningDelivery = GetRulebaseLearningDelivery(appsCoInvestmentRulebaseInfo, learningDelivery);
                    var completedPaymentRecordsInCurrentYear = paymentRecords.Where(p => p.AcademicYear == Generics.AcademicYear && p.TransactionType == 2).ToList();
                    var totalsByPeriodDictionary = BuildCoinvestmentPaymentsPerPeriodDictionary(filteredPaymentRecords);
                    var earliestPaymentInfo = GetEarliestPaymentInfo(paymentRecords);

                    string legalEntityName = null;

                    if (earliestPaymentInfo != null && earliestPaymentInfo.ApprenticeshipId.HasValue)
                    {
                        apprenticeshipIdLegalEntityNameDictionary.TryGetValue(earliestPaymentInfo.ApprenticeshipId.Value, out legalEntityName);
                    }

                    var totalDueCurrentYear = totalsByPeriodDictionary.Sum(d => d.Value);
                    var totalDuePreviousYear = filteredPaymentRecords.Where(p => p.AcademicYear < 1920).Sum(p => p.Amount);
                    var totalCollectedCurrentYear = GetTotalPMRBetweenDates(learningDelivery, _academicYearStart, _nextAcademicYearStart);
                    var totalCollectedPreviousYear = GetTotalPMRBetweenDates(learningDelivery, null, _academicYearStart);

                    var model = new AppsCoInvestmentContributionsModel
                    {
                        Ukprn = ukprn,
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
                        CompletionEarningThisFundingYear = CalculateCompletionEarningsThisFundingYear(learningDelivery, appsCoInvestmentRulebaseInfo),
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
                .Where(row => !IsExcludedRow(row))
                .OrderBy(l => l.LearnRefNumber)
                .ThenBy(t => t.LearningDeliveryFAMTypeApprenticeshipContractType);
        }

        public IDictionary<string, LearnerInfo> BuildLearnerDictionary(AppsCoInvestmentILRInfo ilrInfo)
        {
            return ilrInfo
                .Learners
                .ToDictionary(k => k.LearnRefNumber, v => v, StringComparer.OrdinalIgnoreCase);
        }

        public IDictionary<AppsCoInvestmentRecordKey, List<PaymentInfo>> BuildPaymentInfoDictionary(AppsCoInvestmentPaymentsInfo paymentsInfo)
        {
            return paymentsInfo
                .Payments
                .GroupBy(
                p => new AppsCoInvestmentRecordKey(p.LearnerReferenceNumber, p.LearningStartDate, p.LearningAimProgrammeType, p.LearningAimStandardCode, p.LearningAimFrameworkCode, p.LearningAimPathwayCode), _appsCoInvestmentEqualityComparer)
                .ToDictionary(k => k.Key, v => v.ToList(), _appsCoInvestmentEqualityComparer);
        }

        public bool IsExcludedRow(AppsCoInvestmentContributionsModel row)
        {
            return IsNullOrZero(row.TotalPMRPreviousFundingYears)
                && IsNullOrZero(row.TotalPMRThisFundingYear)
                && IsNullOrZero(row.TotalCoInvestmentDueFromEmployerInPreviousFundingYears)
                && IsNullOrZero(row.TotalCoInvestmentDueFromEmployerThisFundingYear)
                && IsNullOrZero(row.CompletionEarningThisFundingYear)
                && IsNullOrZero(row.CompletionPaymentsThisFundingYear);
        }

        public bool IsNullOrZero(decimal? value) => !value.HasValue || value == 0;

        public IEnumerable<AppsCoInvestmentRecordKey> UnionKeys(ICollection<string> relevantLearnRefNumbers, ICollection<AppsCoInvestmentRecordKey> ilrRecords, ICollection<AppsCoInvestmentRecordKey> paymentsRecords)
        {
            var relevantLearnRefNumbersHashSet = new HashSet<string>(relevantLearnRefNumbers, StringComparer.OrdinalIgnoreCase);

            var filteredRecordsHashSet = new HashSet<AppsCoInvestmentRecordKey>(ilrRecords.Where(r => relevantLearnRefNumbersHashSet.Contains(r.LearnerReferenceNumber)), _appsCoInvestmentEqualityComparer);

            var filteredPaymentRecords = paymentsRecords.Where(r => relevantLearnRefNumbersHashSet.Contains(r.LearnerReferenceNumber));

            foreach (var filteredPaymentRecord in filteredPaymentRecords)
            {
                filteredRecordsHashSet.Add(filteredPaymentRecord);
            }

            return filteredRecordsHashSet;
        }

        public decimal GetPercentageOfInvestmentCollected(decimal? totalDueCurrentYear, decimal? totalDuePreviousYear, decimal? totalCollectedCurrentYear, decimal? totalCollectedPreviousYear)
        {
            var totalDue = (totalDuePreviousYear ?? 0) + (totalDueCurrentYear ?? 0);

            if (totalDue == 0)
            {
                return 0;
            }

            var totalCollected = (totalCollectedPreviousYear ?? 0) + (totalCollectedCurrentYear ?? 0);

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

        public decimal CalculateCompletionEarningsThisFundingYear(LearningDeliveryInfo learningDelivery, AppsCoInvestmentRulebaseInfo rulebaseInfo)
        {
            if (learningDelivery != null)
            {
                return rulebaseInfo?
                    .AECApprenticeshipPriceEpisodePeriodisedValues?
                    .Where(p =>
                        p.LearnRefNumber == learningDelivery.LearnRefNumber
                        && p.AimSeqNumber == learningDelivery.AimSeqNumber
                        && p.Periods != null)
                    .SelectMany(p => p.Periods)
                    .Sum()
                    ?? 0;
            }

            return 0;
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

        public decimal? GetEmployerCoInvestmentPercentage(IEnumerable<PaymentInfo> paymentInfos)
        {
            if (!paymentInfos.Any() || paymentInfos.All(p => p.Amount == 0))
            {
                return null;
            }

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

        public IEnumerable<PaymentInfo> GetPaymentInfosForRecord(IDictionary<AppsCoInvestmentRecordKey, List<PaymentInfo>> paymentsDictionary, AppsCoInvestmentRecordKey record)
        {
            if (paymentsDictionary.TryGetValue(record, out var result))
            {
                return result;
            }

            return Enumerable.Empty<PaymentInfo>();
        }

        public LearningDeliveryInfo GetLearningDeliveryForRecord(LearnerInfo learner, AppsCoInvestmentRecordKey record)
        {
            return learner?
                .LearningDeliveries
                .FirstOrDefault(ld => IlrLearningDeliveryRecordMatch(ld, record));
        }

        public LearnerInfo GetLearnerForRecord(IDictionary<string, LearnerInfo> learnerDictionary, AppsCoInvestmentRecordKey record)
        {
            if (learnerDictionary.TryGetValue(record.LearnerReferenceNumber, out var result))
            {
                return result;
            }

            return null;
        }

        public bool IlrLearningDeliveryRecordMatch(LearningDeliveryInfo learningDelivery, AppsCoInvestmentRecordKey record)
        {
            return learningDelivery.ProgType == record.LearningAimProgrammeType
                    && learningDelivery.StdCode == record.LearningAimStandardCode
                    && learningDelivery.FworkCode == record.LearningAimFrameworkCode
                    && learningDelivery.PwayCode == record.LearningAimPathwayCode
                    && learningDelivery.LearnStartDate == record.LearningStartDate
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
            var fm36learners = ilrInfo
                .Learners?
                .Where(l =>
                    l.LearningDeliveries?
                        .Any(ld => ld.FundModel == 36)
                    ?? false);

            var pmrLearnRefNumbers = fm36learners
                .Where(l =>
                    l.LearningDeliveries?
                        .Any(ld => ld.AppFinRecords?.Any(afr => afr.AFinType == "PMR") ?? false)
                        ?? false)
                .Select(l => l.LearnRefNumber).ToList()
                ?? Enumerable.Empty<string>();

            var fm36LearnRefNumbers = new HashSet<string>(fm36learners.Select(l => l.LearnRefNumber), StringComparer.OrdinalIgnoreCase);

            var paymentLearnRefNumbers = paymentsInfo
                .Payments
                .Where(p => p.FundingSource == _fundingSource && fm36LearnRefNumbers.Contains(p.LearnerReferenceNumber))
                .Select(p => p.LearnerReferenceNumber).ToList();

            return pmrLearnRefNumbers.Union(paymentLearnRefNumbers);
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
                        && p.LearnRefNumber.CaseInsensitiveEquals(learnRefNumber)
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