using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ESFA.DC.PeriodEnd.ReportService.Reports.AppsMonthly.Model;
using ESFA.DC.PeriodEnd.ReportService.Reports.Constants;
using ESFA.DC.PeriodEnd.ReportService.Reports.Extensions;
using ESFA.DC.PeriodEnd.ReportService.Reports.Interface.AppsCoInvestment.Builders;
using ESFA.DC.PeriodEnd.ReportService.Reports.Interface.AppsCoInvestment.Model;

namespace ESFA.DC.PeriodEnd.ReportService.Reports.AppsCoInvestment
{
    public class AppsCoInvestmentModelBuilder : IAppsCoInvestmentModelBuilder
    {
        private const string NotApplicable = "Not applicable.  For more information refer to the funding reports guidance.";
        private readonly IEqualityComparer<AppsCoInvestmentRecordKey> _appsCoInvestmentEqualityComparer;
        private readonly IPaymentsBuilder _paymentsBuilder;
        private readonly ILearnersBuilder _learnersBuilder;
        private readonly ILearningDeliveriesBuilder _learningDeliveriesBuilder;
        private const int FundingSource = 3;
        private const int FundingModel = 36;
        private readonly HashSet<int> _transactionTypes = new HashSet<int>
        {
            DASPayments.TransactionType.Learning_On_Programme,
            DASPayments.TransactionType.Completion,
            DASPayments.TransactionType.Balancing,
        };
        private const int _currentAcademicYear = 2021;
        private readonly DateTime _academicYearStart = new DateTime(2020, 8, 1);
        private readonly DateTime _nextAcademicYearStart = new DateTime(2021, 8, 1);
       
        public AppsCoInvestmentModelBuilder(
            IEqualityComparer<AppsCoInvestmentRecordKey> appsCoInvestmentEqualityComparer,
            IPaymentsBuilder paymentsBuilder,
            ILearnersBuilder learnersBuilder,
            ILearningDeliveriesBuilder learningDeliveriesBuilder)
        {
            _appsCoInvestmentEqualityComparer = appsCoInvestmentEqualityComparer;
            _paymentsBuilder = paymentsBuilder;
            _learnersBuilder = learnersBuilder;
            _learningDeliveriesBuilder = learningDeliveriesBuilder;
        }

        public IEnumerable<AppsCoInvestmentRecord> Build(
            ICollection<Learner> learners,
            ICollection<Payment> payments,
            ICollection<AECApprenticeshipPriceEpisodePeriodisedValues> aecPriceEpisodePeriodisedValues)
        {

            var paymentsDictionary = _paymentsBuilder.BuildPaymentsLookupDictionary(payments);
            var learnerDictionary = _learnersBuilder.BuildLearnerLookUpDictionary(learners);
            var relevantLearnRefNumbers = GetRelevantLearners(learners, payments).ToList();
            var ilrAppsCoInvestmentUniqueKeys = _learnersBuilder.GetUniqueAppsCoInvestmentRecordKeysAsync(learners);
            var paymentsAppsCoInvestmentUniqueKeys = _paymentsBuilder.GetUniqueCombinationsOfKeyFromPaymentsAsync(payments);

            List<AppsCoInvestmentRecordKey> uniqueKeys = UnionKeys(relevantLearnRefNumbers, ilrAppsCoInvestmentUniqueKeys, paymentsAppsCoInvestmentUniqueKeys).ToList();

            return uniqueKeys
                .Where(r => FilterReportRows(payments, aecPriceEpisodePeriodisedValues, learners, r))
                .Select(record =>
                {
                    var allPayments = _paymentsBuilder.GetPaymentsForRecord(paymentsDictionary, record).ToList();
                    var learner = _learnersBuilder.GetLearnerForRecord(learnerDictionary, record);
                    var learningDelivery = _learningDeliveriesBuilder.GetLearningDeliveryForRecord(learner, record);
                    var filteredPayments = _paymentsBuilder.FilterByFundingSourceAndTransactionType(allPayments, FundingSource, _transactionTypes).ToList();
                    var earliestPaymentInfo = _paymentsBuilder.GetEarliestPayment(allPayments);
                    var familyName = GetName(learner, l => l.FamilyName);
                    var givenNames = GetName(learner, l => l.GivenNames);

                    var model = new AppsCoInvestmentRecord()
                    {
                        RecordKey = record,
                        FamilyName = familyName,
                        GivenNames = givenNames,
                        UniqueLearnerNumber = GetUniqueOrEmpty(allPayments, p => p.LearnerUln),
                        LearningDelivery = learningDelivery,
                        ApprenticeshipContractType = GetUniqueOrEmpty(allPayments, p => p.ContractType),
                        EmployerIdentifierAtStartOfLearning = _learnersBuilder.GetEmploymentStatus(learner, record.LearningStartDate),
                        EmployerNameFromApprenticeshipService = earliestPaymentInfo?.LegalEntityName,
                        LDM356Or361 = _learningDeliveriesBuilder.HasLdm356Or361(learningDelivery) ? "Yes" : "No",
                        EarningsAndPayments = _paymentsBuilder.BuildEarningsAndPayments(filteredPayments, allPayments, learningDelivery, aecPriceEpisodePeriodisedValues, _currentAcademicYear, _academicYearStart, _nextAcademicYearStart)
                     };

                    return model;
                })
                .Where(row => !IsExcludedRow(row))
                .OrderBy(l => l.RecordKey.LearnerReferenceNumber)
                .ThenBy(t => t.ApprenticeshipContractType);
        }

        public string GetName(Learner learner, Func<Learner, string> selector)
            => learner == null ? NotApplicable : selector(learner);

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
        
        public bool IsExcludedRow(AppsCoInvestmentRecord row)
        {
            return IsNullOrZero(row.EarningsAndPayments.TotalPMRPreviousFundingYears)
                   && IsNullOrZero(row.EarningsAndPayments.TotalPMRThisFundingYear)
                   && IsNullOrZero(row.EarningsAndPayments.TotalCoInvestmentDueFromEmployerInPreviousFundingYears)
                   && IsNullOrZero(row.EarningsAndPayments.TotalCoInvestmentDueFromEmployerThisFundingYear)
                   && IsNullOrZero(row.EarningsAndPayments.CompletionEarningThisFundingYear)
                   && IsNullOrZero(row.EarningsAndPayments.CompletionPaymentsThisFundingYear);
        }

        public bool IsNullOrZero(decimal? value) => !value.HasValue || value == 0;

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
        public IEnumerable<string> GetRelevantLearners(ICollection<Learner> learners, ICollection<Payment> payments)
        {
            var fm36Learners = learners?
                .Where(l =>
                    l.LearningDeliveries?
                        .Any(ld => ld.FundModel == FundingModel)
                    ?? false);

            var pmrLearnRefNumbers = fm36Learners
                                         .Where(l =>
                                             l.LearningDeliveries?
                                                 .Any(ld => ld.AppFinRecords?.Any(afr => afr.AFinType == FinTypes.PMR) ?? false)
                                             ?? false)
                                         .Select(l => l.LearnRefNumber).ToList()
                                     ?? Enumerable.Empty<string>();

            var fm36LearnRefNumbers = new HashSet<string>(fm36Learners.Select(l => l.LearnRefNumber), StringComparer.OrdinalIgnoreCase);

            var paymentLearnRefNumbers = payments
                .Where(p => p.FundingSource == FundingSource && fm36LearnRefNumbers.Contains(p.LearnerReferenceNumber))
                .Select(p => p.LearnerReferenceNumber).ToList();

            return pmrLearnRefNumbers.Union(paymentLearnRefNumbers);
        }

        // BR2
        public bool FilterReportRows(ICollection<Payment> payments, ICollection<AECApprenticeshipPriceEpisodePeriodisedValues> aecApprenticeshipPriceEpisodePeriodisedValues, ICollection<Learner> learners, AppsCoInvestmentRecordKey recordKey)
        {
            return
                EmployerCoInvestmentPaymentFilter(payments, recordKey.LearnerReferenceNumber)
                || CompletionPaymentFilter(payments, recordKey.LearnerReferenceNumber)
                || PMRAppFinRecordFilter(learners, recordKey.LearnerReferenceNumber)
                || NonZeroCompletionEarningsFilter(aecApprenticeshipPriceEpisodePeriodisedValues, recordKey.LearnerReferenceNumber);
        }

        public bool EmployerCoInvestmentPaymentFilter(ICollection<Payment> payments, string learnRefNumber)
        {
            return payments?.Any(p => p.FundingSource == 3 && p.LearnerReferenceNumber.CaseInsensitiveEquals(learnRefNumber)) ?? false;
        }

        public bool CompletionPaymentFilter(ICollection<Payment> payments, string learnRefNumber)
        {
            return payments?.Any(p => p.TransactionType == 3 && p.LearnerReferenceNumber.CaseInsensitiveEquals(learnRefNumber)) ?? false;
        }

        public bool PMRAppFinRecordFilter(ICollection<Learner> learners, string learnRefNumber)
        {
            return learners?.Any(
                           l =>
                               l.LearnRefNumber.CaseInsensitiveEquals(learnRefNumber)
                               && (l.LearningDeliveries?.Any(ld =>
                                       ld.AppFinRecords?.Any(afr => afr.AFinType.CaseInsensitiveEquals(FinTypes.PMR))
                                       ?? false)
                                   ?? false))
                   ?? false;
        }

        public bool NonZeroCompletionEarningsFilter(ICollection<AECApprenticeshipPriceEpisodePeriodisedValues> aecApprenticeshipPriceEpisodePeriodisedValues, string learnRefNumber)
        {
            return aecApprenticeshipPriceEpisodePeriodisedValues?
                       .Any(
                           p =>
                               p.AttributeName == AttributeConstants.Fm36PriceEpisodeCompletionPaymentAttributeName
                               && p.LearnRefNumber.CaseInsensitiveEquals(learnRefNumber)
                               && (p.Periods?.Any(v => v.HasValue && v != 0) ?? false))
                   ?? false;
        }
    }
}
